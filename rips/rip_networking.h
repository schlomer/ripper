/**************************************
  Rip Server (C) 2019 Shannon Schlomer
 **************************************/

#ifndef RIP_NETWORKING_H
#define RIP_NETWORKING_H

#include "rip_data.h"

#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.Networking.Sockets.h>
#include <winrt/Windows.Storage.Streams.h>
#include <sstream>
#include <future>
#include <experimental/generator>

using namespace winrt;
using namespace Windows::Foundation;
using namespace Windows::Storage::Streams;
using namespace Windows::Networking::Sockets;

namespace rip
{
	namespace networking
	{
		// request_types
		enum class request_types 
		{
			add_data_server = 1,
			add_database = 2,
			add_container = 3,
			set_record = 4,
			delete_records = 5,
			add_index = 6,
			get_record = 7,
			get_records = 8,
			get_data_server = 9,
			get_database = 10,
			get_container = 11
		};

		// server
		class server
		{
			std::shared_ptr<configuration> config;

			std::unordered_map<std::string, std::shared_ptr<data::server>> data_servers;
			StreamSocketListener stream_socket_listener;
			StreamSocket stream_socket;
			std::string name;

			IAsyncAction handle_connection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args);
			IAsyncAction on_connection_received_async(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args);
			size_t load_transaction_logs(std::shared_ptr<data::server>& data_server);
			json handle_add_data_server(const json& message);
			json handle_add_database(const json& message, bool log);
			json handle_add_container(const json& message, bool log);
			json handle_get_data_server(const json& message);
			json handle_get_database(const json& message);
			json handle_get_container(const json& message);
			json handle_set_record(const json& message, bool log);
			json handle_get_record(const json& message);
			json handle_delete_records(const json& message, bool log);
			json handle_add_index(const json& message, bool log);
			std::experimental::generator<std::string> handle_get_records(const json& message);
			std::shared_ptr<data::container> get_container(std::string data_server_name, std::string database_name, std::string container_name
				,std::shared_ptr<data::server>& server_out);

		public:
			server(std::shared_ptr<configuration>& config, std::string name);
			IAsyncAction start_async();
		};
	}
}

#endif