/**************************************
  Rip Server (C) 2019 Shannon Schlomer
 **************************************/

#include "pch.h"

using namespace rip;
using namespace rip::data;

// container::partition
void container::partition::unsafe_set_record(std::string id, const json& data)
{		
	records[id] = data.dump();	
}

std::string container::partition::get_record(std::string id) const
{
	std::shared_lock lg(mtx_records);
	auto r = records.find(id);
	if (r == records.end())
		return std::string();

	lg.unlock();

	return r->second;	
}

void container::partition::delete_record(std::string id, bool& partition_is_empty)
{
	std::unique_lock lg(mtx_records);
	
	partition_is_empty = false;
	auto r = records.find(id);
	if (r == records.end())
		return;
	
	records.erase(r);
	unsafe_delete_from_indexes(id);
	partition_is_empty = records.empty();	
}

void container::partition::delete_records()
{
	std::unique_lock lg(mtx_records);	
	
	records.clear();
	indexes.clear();
	rev_indexes.clear();
}

void container::partition::unsafe_add_indexes(std::vector<std::string> paths)
{	
	for (auto& path : paths)
	{
		auto i = indexes.find(path);
		if (i == indexes.end())
		{
			indexes[path] = std::make_shared<std::multimap<json, std::string>>();
			rev_indexes[path] = std::make_shared<std::unordered_map<std::string, json>>();
		}
	}
}

// container
container::container(std::string name, std::string partition_key_path, std::string id_path) 
	: name(name), partition_key_path(partition_key_path), id_path(id_path)
{
	
}

std::string container::get_record(std::string partition_key, std::string id) const
{
	std::shared_lock lg(mtx_partitions);
	auto p = partitions.find(partition_key);
	if (p == partitions.end())
		return std::string();

	auto pp = p->second;

	lg.unlock();

	return pp->get_record(id);
}

std::experimental::generator<std::string> container::get_records() const
{		
	std::shared_lock lg(mtx_partitions);
			
	for (auto& i : partitions)
	{	
		std::shared_lock lgr(i.second->mtx_records);		
		for (auto& r : i.second->records)
		{
			co_yield r.second;		
		}
	}	
}

std::experimental::generator<std::string> container::get_records(std::string partition_key) const
{
	std::shared_lock lg(mtx_partitions);
	auto p = partitions.find(partition_key);
	if (p == partitions.end())
		co_return;

	auto pp = p->second;

	lg.unlock();

	std::shared_lock lgr(pp->mtx_records);
	for (auto& r : pp->records)
	{
		co_yield r.second;		
	}
}

std::experimental::generator<std::string> container::get_records(std::string partition_key, std::string index_path, const json& begin_value, const json& end_value) const
{
	if (end_value < begin_value)
		co_return;

	std::shared_lock lg(mtx_partitions);
	auto p = partitions.find(partition_key);
	if (p == partitions.end())
		co_return;

	auto pp = p->second;

	lg.unlock();

	std::shared_lock lgr(pp->mtx_records);
	auto i = pp->indexes.find(index_path);
	if (i == pp->indexes.end())
		return;
		
	auto low = i->second->lower_bound(begin_value);
	auto high = i->second->upper_bound(end_value);
	if (low != i->second->end())
	{
		for (auto x = low; x != high; ++x)
		{	
			co_yield pp->get_record(x->second);
		}
	}
}

std::experimental::generator<std::string> container::get_records(std::string partition_key, std::string index_path, const json& value) const
{
	std::shared_lock lg(mtx_partitions);
	auto p = partitions.find(partition_key);
	if (p == partitions.end())
		co_return;

	auto pp = p->second;

	lg.unlock();

	std::shared_lock lgr(pp->mtx_records);
 	auto i = pp->indexes.find(index_path);
	if (i == pp->indexes.end())
		co_return;

	auto range = i->second->equal_range(value);	
	for(auto x = range.first; x != range.second; ++x)
	{		
		co_yield pp->get_record(x->second);
	}
}

std::experimental::generator<std::string> container::get_records(std::string index_path, const json& value) const
{
	std::shared_lock lg(mtx_partitions);
	for (auto& p : partitions)
	{
		std::shared_lock lgr(p.second->mtx_records);
		auto i = p.second->indexes.find(index_path);
		if (i == p.second->indexes.end())
			continue;

		auto range = i->second->equal_range(value);
		for (auto x = range.first; x != range.second; ++x)
		{			
			co_yield p.second->get_record(x->second);
		}
	}
}

std::experimental::generator<std::string> container::get_records(std::string index_path, const json& begin_value, const json& end_value) const
{
	if (end_value < begin_value)
		co_return;

	std::shared_lock lg(mtx_partitions);
	for (auto& p : partitions)
	{
		std::shared_lock lgr(p.second->mtx_records);
		auto i = p.second->indexes.find(index_path);
		if (i == p.second->indexes.end())
			return;

		auto low = i->second->lower_bound(begin_value);
		auto high = i->second->upper_bound(end_value);
		if (low != i->second->end())
		{
			for (auto x = low; x != high; ++x)
			{				
				co_yield p.second->get_record(x->second);
			}
		}
	}
}

void container::set_record(const json& value)
{	
	auto j_partition_key = value.at(json::json_pointer(partition_key_path));
	auto j_id = value.at(json::json_pointer(id_path));

	if (j_partition_key.is_null() || j_id.is_null())
		return;

	auto partition_key = j_partition_key.get<std::string>();
	auto id = j_id.get<std::string>();
	
	std::shared_lock lg(mtx_partitions);
	auto p = partitions.find(partition_key);
	if (p == partitions.end())
	{
		lg.unlock();		
		std::unique_lock lgu(mtx_partitions);		
		auto new_p = std::make_shared<partition>();
		partitions[partition_key] = new_p;
		lgu.unlock();
		std::unique_lock lgr(new_p->mtx_records);
		new_p->unsafe_set_record(id, value); 
		new_p->unsafe_add_indexes(index_paths);
		new_p->unsafe_update_indexes(id, value);
	}
	else
	{
		lg.unlock();
		std::unique_lock lgr(p->second->mtx_records);
		p->second->unsafe_set_record(id, value); 
		p->second->unsafe_update_indexes(id, value);
	}
}

void container::partition::unsafe_update_indexes(std::string id, const json& value)
{
	for (auto& index : indexes)
	{
		auto ri = rev_indexes.find(index.first);
		auto rii = ri->second->find(id);
		if (rii != ri->second->end())
		{
			auto iid = index.second->equal_range(rii->second);
			// remove id from index
			for (auto i = iid.first; i != iid.second; ++i)
			{
				if (i->second == id)
				{
					index.second->erase(i);
					break;
				}
			}

			ri->second->erase(rii);
		}

		// add id to index
		auto jp = value.at(json::json_pointer(index.first));
		if (jp.is_null() == false)
		{			
			index.second->insert(std::make_pair(jp, id));
			ri->second->insert(std::make_pair(id, jp));
		}
	}
}

void container::partition::unsafe_delete_from_indexes(std::string id)
{
	for (auto& index : indexes)
	{
		auto ri = rev_indexes.find(index.first);
		auto rii = ri->second->find(id);
		if (rii != ri->second->end())
		{
			auto iid = index.second->equal_range(rii->second);
			// remove id from index
			for (auto i = iid.first; i != iid.second; ++i)
			{
				if (i->second == id)
				{
					index.second->erase(i);
					break;
				}
			}

			ri->second->erase(rii);
		}
	}
}

void container::add_index(std::string path)
{
	auto i = std::find(index_paths.begin(), index_paths.end(), path);
	if(i == index_paths.end())
		index_paths.push_back(path);
}

void container::delete_record(std::string partition_key, std::string id)
{	
	std::shared_lock lg(mtx_partitions);
	auto p = partitions.find(partition_key);
	if (p == partitions.end())
		return;

	auto pp = p->second;

	lg.unlock();

	bool partition_is_empty;
	pp->delete_record(id, partition_is_empty);
	if (partition_is_empty)
	{
		std::unique_lock lgu(mtx_partitions);
		p = partitions.find(partition_key);
		if (p != partitions.end())
			partitions.erase(p);
	}
}

void container::delete_records(std::string partition_key)
{
	std::unique_lock lgu(mtx_partitions);
	auto p = partitions.find(partition_key);
	if (p != partitions.end())
	{
		p->second->delete_records();
		partitions.erase(p);
	}
}

void container::delete_records()
{
	std::unique_lock lg(mtx_partitions);
	partitions.clear();
}

std::string container::get_name() const
{
	return name;
}

std::string container::get_partition_key_path() const
{
	return partition_key_path;
}
std::string container::get_id_path() const
{
	return id_path;
}

std::vector<std::string> container::get_index_paths() const
{
	return index_paths;
}
