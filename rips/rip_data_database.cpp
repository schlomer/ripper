/**************************************
  Rip Server (C) 2019 Shannon Schlomer
 **************************************/

#include "pch.h"

using namespace rip;
using namespace rip::data;

// database
database::database(std::string name) : name(name)
{

}

std::shared_ptr<container> database::add_container(std::string _name, std::string partition_key_path, std::string id_path)
{
	std::lock_guard lg(mtx_containers);
	auto i = containers.find(_name);
	if (i != containers.end())
		return i->second;

	auto c = std::make_shared<container>(_name, partition_key_path, id_path);
	containers.insert(std::make_pair(_name, c));	
	return c;
}

std::shared_ptr<container> database::get_container(std::string _name) const
{
	std::lock_guard lg(mtx_containers);
	auto i = containers.find(_name);
	if (i == containers.end())
		return nullptr;
	return i->second;
}

std::string database::get_name() const
{
	return name;
}

std::vector<std::shared_ptr<container>> database::get_containers() const
{
	std::lock_guard lg(mtx_containers);

	std::vector<std::shared_ptr<container>> r;
	for (auto& i : containers)
		r.push_back(i.second);
	
	return r;
}