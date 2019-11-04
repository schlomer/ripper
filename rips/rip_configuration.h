/**************************************
  Rip Server (C) 2019 Shannon Schlomer
 **************************************/

#ifndef RIP_CONFIG_H
#define RIP_CONFIG_H

#include <string>

#include "json.hpp"
using namespace nlohmann;

namespace rip
{
	// configuration
	class configuration
	{
		std::string path;
		json content;

		std::string server_name;
		uint32_t tcp_port;
		uint32_t tcp_send_size_bytes;
		std::string transaction_log_path;

	public:
		configuration();
		bool load(std::string path);
		std::string get_path() const { return path; }
		std::string get_server_name() const { return server_name; }
		uint32_t get_tcp_port() const { return tcp_port; }
		uint32_t get_tcp_send_size_bytes() const { return tcp_send_size_bytes; }
		std::string get_transaction_log_path() const { return transaction_log_path; }
	};
}


#endif
