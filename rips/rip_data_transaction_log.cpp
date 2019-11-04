/**************************************
  Rip Server (C) 2019 Shannon Schlomer
 **************************************/

#include "pch.h"

using namespace rip;
using namespace rip::data;

#include <sstream>
#include <random>
#include <string>
#include <chrono>
using namespace std::chrono;

#include <fstream>
#include <filesystem>


void transaction_log::close()
{
	file.close();
}

void transaction_log::open()
{
	file.open(path, std::ios::out | std::ios::app | std::ios::binary);
	if (file.fail())
	{
		char error[500];
		strerror_s(error, 499, errno);
		throw std::ios_base::failure(error);
	}

	file.exceptions(file.exceptions() | std::ios::failbit | std::ifstream::badbit);
}

transaction_log::transaction_log(std::string path) : path(path), next_id(0)
{	
	open();
}

uint64_t transaction_log::begin()
{
	return std::atomic_fetch_add(&next_id, 1);
}

void transaction_log::commit(uint64_t id)
{
	spin_lock_guard lg(spin);

	int64_t timestamp = duration_cast<milliseconds>(system_clock::now().time_since_epoch()).count();

	json tx = json::object();
	tx["t"] = timestamp;
	tx["x"] = id;
	tx["c"] = "commit";

	std::vector<uint8_t> vtx = json::to_msgpack(tx);
	uint32_t vtxlen = (uint32_t)vtx.size();
	file.write(reinterpret_cast<char*>(&vtxlen), sizeof(vtxlen));
	file.write(reinterpret_cast<char*>(&vtx[0]), vtxlen * sizeof(uint8_t));	
	file.flush();
}

void transaction_log::write(uint64_t id, const json& j)
{	
	spin_lock_guard lg(spin);

	int64_t timestamp = duration_cast<milliseconds>(system_clock::now().time_since_epoch()).count();	

	json tx = json::object();	
	tx["c"] = j;	
	tx["t"] = timestamp;
	tx["x"] = id;	

	std::vector<uint8_t> vtx = json::to_msgpack(tx);
	uint32_t vtxlen = (uint32_t)vtx.size();
	file.write(reinterpret_cast<char*>(&vtxlen), sizeof(vtxlen));
	file.write(reinterpret_cast<char*>(&vtx[0]), vtxlen * sizeof(uint8_t));	
	file.flush();
}

void transaction_log::set_next_id(uint64_t id)
{
	std::atomic_store(&next_id, id);
}

std::experimental::generator<json> transaction_log::get_logs()
{
	if (std::filesystem::exists(this->path) == false)
		return;

	std::ifstream readfile;
	readfile.open(path, std::ios::in | std::ios::binary);	

	readfile.seekg(0, std::ios::end);
	if (readfile.tellg() == 0)
		return;

	if (readfile.fail())
	{
		char error[500];
		strerror_s(error, 499, errno);
		throw std::ios_base::failure(error);
	}

	readfile.exceptions(readfile.exceptions() | std::ifstream::badbit);

	std::vector<uint8_t> vtx;
	readfile.seekg(0, std::ios::beg);
	try
	{
		close();
		while (true)
		{
			uint32_t vtxlen;
			if (readfile.read(reinterpret_cast<char*>(&vtxlen), sizeof(vtxlen)))
			{
				vtx.resize(vtxlen);
				if (readfile.read(reinterpret_cast<char*>(&vtx[0]), vtxlen * sizeof(uint8_t)))
				{

					co_yield json::from_msgpack(vtx);
				}
				else throw std::exception("Corrupt transaction log.");
			}
			else break;
		}
		open();
	}
	catch(...)
	{
		open();
		throw;
	}
}