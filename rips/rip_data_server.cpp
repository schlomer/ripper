/**************************************
  Rip Server (C) 2019 Shannon Schlomer
 **************************************/

#include "pch.h"

using namespace rip;
using namespace rip::data;

// server

server::server(std::string name, std::string txlog_path) : name(name), txlog_path(txlog_path)
{
	txlog = std::make_shared<transaction_log>(name + "-" + txlog_path);
}

std::string server::get_name() const
{
	return name;
}

std::string server::get_txlog_path() const
{
	return txlog_path;
}

std::shared_ptr<transaction_log> server::get_txlog()
{
	return txlog;
}

std::shared_ptr<database> server::add_database(std::string _name)
{
	std::lock_guard lg(mtx_databases);

	auto i = databases.find(_name);
	if (i != databases.end())		
		return i->second;

	auto d = std::make_shared<database>(_name);	
	databases.insert(std::make_pair(_name, d));
	
	return d;
}

std::shared_ptr<database> server::get_database(std::string _name) const
{
	std::lock_guard lg(mtx_databases);

	auto i = databases.find(_name);
	if (i == databases.end())
		return nullptr;
	return i->second;
}

std::vector<std::shared_ptr<database>> server::get_databases() const
{
	std::lock_guard lg(mtx_databases);

	std::vector<std::shared_ptr<database>> r;
	for (auto& i : databases)
		r.push_back(i.second);
	
	return r;
}