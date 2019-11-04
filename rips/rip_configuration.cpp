/**************************************
  Rip Server (C) 2019 Shannon Schlomer
 **************************************/

#include "pch.h"

#include <fstream>
#include <iostream>
#include <sstream>

using namespace rip;

configuration::configuration() : tcp_port(0), tcp_send_size_bytes(65535)
{

}

bool configuration::load(std::string _path)
{
	try
	{
		if (_path.length() == 0)
			return false;

		path = _path;

		std::ifstream file;
		file.open(path);

		std::stringstream ss;
		ss << file.rdbuf(); 
		auto s = ss.str();

		content = json::parse(s);

		server_name = content["serverName"];
		tcp_port = content["tcpPort"];
		tcp_send_size_bytes = content["tcpSendSizeBytes"];
		transaction_log_path = content["transactionLogPath"];

		return true;
	}
	catch (std::exception&)
	{
		return false;
	}
	catch (...)
	{
		return false;
	}
}
