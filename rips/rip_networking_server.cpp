/**************************************
  Rip Server (C) 2019 Shannon Schlomer
 **************************************/

#include "pch.h"

#include "rip_networking.h"
#include "json.hpp"

using namespace rip;
using namespace rip::networking;


static std::string get_socket_error_string(SocketErrorStatus s)
{
	switch (s)
	{
	case SocketErrorStatus::Unknown: return "unknown";
	case SocketErrorStatus::OperationAborted: return "operation aborted";
	case SocketErrorStatus::HttpInvalidServerResponse: return "invalid http server response";
	case SocketErrorStatus::ConnectionTimedOut: return "connection timed out";
	case SocketErrorStatus::AddressFamilyNotSupported: return "address family not supporte";
	case SocketErrorStatus::SocketTypeNotSupported: return "socket type not supported";
	case SocketErrorStatus::HostNotFound: return "host not found";
	case SocketErrorStatus::NoDataRecordOfRequestedType: return "no data record of requested type";
	case SocketErrorStatus::NonAuthoritativeHostNotFound: return "non-authoritative host not found";
	case SocketErrorStatus::ClassTypeNotFound: return "class type not found";
	case SocketErrorStatus::AddressAlreadyInUse: return "address already in use";
	case SocketErrorStatus::CannotAssignRequestedAddress: return "cannot assign requested address";
	case SocketErrorStatus::ConnectionRefused: return "connection refused";
	case SocketErrorStatus::NetworkIsUnreachable: return "network is unreachable";
	case SocketErrorStatus::UnreachableHost: return "unreachable host";
	case SocketErrorStatus::NetworkIsDown: return "network is down";
	case SocketErrorStatus::NetworkDroppedConnectionOnReset: return "network droppped connection on reset";
	case SocketErrorStatus::SoftwareCausedConnectionAbort: return "software caused connection abort";
	case SocketErrorStatus::ConnectionResetByPeer: return "connection reset by peer";
	case SocketErrorStatus::HostIsDown: return "host is down";
	case SocketErrorStatus::NoAddressesFound: return "no addresses found";
	case SocketErrorStatus::TooManyOpenFiles: return "too many files open";
	case SocketErrorStatus::MessageTooLong: return "message too long";
	case SocketErrorStatus::CertificateExpired: return "certificate expired";
	case SocketErrorStatus::CertificateUntrustedRoot: return "certificate untrusted root";
	case SocketErrorStatus::CertificateCommonNameIsIncorrect: return "certificate common name is incorrect";
	case SocketErrorStatus::CertificateWrongUsage: return "certificate wrong usage";
	case SocketErrorStatus::CertificateRevoked: return "certificate revoked";
	case SocketErrorStatus::CertificateNoRevocationCheck: return "certificate no revocation check";
	case SocketErrorStatus::CertificateRevocationServerOffline: return "certificate revocation server offline";
	case SocketErrorStatus::CertificateIsInvalid: return "certificate is invalid";
	default:
		return "unknown error";
	};
}

server::server(std::shared_ptr<configuration>& config, std::string name) : config(config), name(name)
{
}

IAsyncAction server::start_async()
{
	try
	{	
		stream_socket_listener.ConnectionReceived({ this, &server::on_connection_received_async });
		co_await stream_socket_listener.BindServiceNameAsync(std::to_wstring(config->get_tcp_port()));		

		console_line(config->get_server_name(), "Listening on port " + std::to_string(config->get_tcp_port()));		

	}
	catch (winrt::hresult_error const& ex)
	{
		SocketErrorStatus error_status{ Windows::Networking::Sockets::SocketError::GetStatus(ex.to_abi()) };

		console_error_line(config->get_server_name(), "Socket Error", get_socket_error_string(error_status));		
	}
}

json ok_response()
{
	json r = json::object();
	r["e"] = nullptr;
	return r;
}

json error_response(std::string message)
{
	json r = json::object();
	r["e"] = message;
	return r;
}

json missing_argument_response(std::string argumentName)
{
	return error_response("Missing argument: " + argumentName);
}

json invalid_argument_response(std::string argumentName)
{
	return error_response("Invalid argument: " + argumentName);
}

static const char* key_request_type = "rt";
static const char* key_data_server_name = "sn";
static const char* key_database_name = "dn";
static const char* key_container_name = "cn";

json validate_required_key(const json& message, std::string key, std::string& key_value_out)
{
	if (message.contains(key) == false)
		return missing_argument_response(key);

	key_value_out = message[key];
	if (key_value_out.length() == 0)
		return invalid_argument_response(key);

	return nullptr;
}

json get_container_path_parts(const json& message, std::string& data_server_name_out, std::string& database_name_out, std::string& container_name_out)
{	
	json v = validate_required_key(message, key_data_server_name, data_server_name_out);
	if (v.is_null() == false)
		return v;
	
	v = validate_required_key(message, key_database_name, database_name_out);
	if (v.is_null() == false)
		return v;

	v = validate_required_key(message, key_container_name, container_name_out);
	if (v.is_null() == false)
		return v;

	return nullptr;
}

size_t server::load_transaction_logs(std::shared_ptr<data::server>& data_server)
{
	auto txlog = data_server->get_txlog();

	size_t log_count = 0;
	uint64_t max_tx = 0;

	std::unordered_map<uint64_t, json> to_commit;

	for (auto j : txlog->get_logs())
	{
		++log_count;
		
		json json_response;

		if (j.contains("c") == false)
			throw std::exception("Transaction log invalid.");

		json c = j["c"];
				
		if (c.is_string() && "commit" == c.get<std::string>())
		{
			uint64_t tx = j["x"];
			auto i = to_commit.find(tx);
			if (i != to_commit.end())
			{
				c = i->second["c"];

				uint32_t request_type = c[key_request_type];

				switch ((request_types)request_type)
				{
				case request_types::set_record:
					json_response = handle_set_record(c, false);
					break;
				case request_types::delete_records:
					json_response = handle_delete_records(c, false);
					break;
				case request_types::add_index:
					json_response = handle_add_index(c, false);
					break;
				case request_types::add_container:
					json_response = handle_add_container(c, false);
					break;
				case request_types::add_database:
					json_response = handle_add_database(c, false);
					break;
				}				

				if (json_response != nullptr)
				{
					if (json_response.contains("e") == false)
						throw std::exception("Invalid transaction log execution.");
					if (json_response["e"] != nullptr)
						throw std::exception(json_response["e"].get<std::string>().c_str());
				}
			}
		}
		else
		{
			if (c.contains(key_request_type) == false)
				throw std::exception("Transaction log is corrupt.");

			uint64_t tx = j["x"];
			to_commit[tx] = j;

			if (tx > max_tx)
				max_tx = tx;			
		}
	}

	txlog->set_next_id(max_tx+1);

	return log_count;
}

json server::handle_add_data_server(const json& message)
{
	std::string data_server_name;
	json v = validate_required_key(message, key_data_server_name, data_server_name);
	if (v.is_null() == false)
		return v;

	auto s = data_servers.find(data_server_name);
	if (s == data_servers.end())
	{
		auto data_server = std::make_shared<data::server>(data_server_name, config->get_transaction_log_path());
		data_servers.insert(std::make_pair(data_server_name, data_server));
		auto txlog = data_server->get_txlog();
		console_line(config->get_server_name(), "Loading transaction logs for " + data_server_name + "...");
		auto txlog_count = load_transaction_logs(data_server);
		if(txlog_count == 0)
		{
			console_line(config->get_server_name(), "No transaction logs found for " + data_server_name + ". Creating transaction logs.");
			auto tx = txlog->begin();
			txlog->write(tx, message);
			txlog->commit(tx);
		}
		else console_line(config->get_server_name(), data_server_name + " transaction logs loaded.");

		return ok_response();
	}
	else
	{	
		return error_response("Data server already exists: " + data_server_name);
	}
}

json server::handle_add_database(const json& message, bool log)
{
	std::string data_server_name;
	json v = validate_required_key(message, key_data_server_name, data_server_name);
	if (v.is_null() == false)
		return v;

	std::string database_name;
	v = validate_required_key(message, key_database_name, database_name);
	if (v.is_null() == false)
		return v;	

	auto s = data_servers.find(data_server_name);
	if (s != data_servers.end())
	{
		if (log)
		{			
			auto txlog = s->second->get_txlog();
			auto tx = txlog->begin();
			txlog->write(tx, message);
			
			auto db = s->second->add_database(database_name);
			
			txlog->commit(tx);
		}
		else
		{
			auto db = s->second->add_database(database_name);
		}

		return ok_response();
	}
	else return error_response("Data server not found: " + data_server_name);			
}

json server::handle_add_container(const json& message, bool log)
{
	std::string data_server_name, database_name, container_name;
	
	json v = get_container_path_parts(message, data_server_name, database_name, container_name);
	if (v.is_null() == false)
		return v;

	std::string partition_key_path;
	v = validate_required_key(message, "pkp", partition_key_path);
	if (v.is_null() == false)
		return v;

	std::string id_path;
	v = validate_required_key(message, "idp", id_path);
	if (v.is_null() == false)
		return v;

	auto s = data_servers.find(data_server_name);
	if (s != data_servers.end())
	{
		auto d = s->second->get_database(database_name);
		if (d != nullptr)
		{	
			if (log)
			{				
				auto txlog = s->second->get_txlog();
				auto tx = txlog->begin();
				txlog->write(tx, message);

				auto c = d->add_container(container_name, partition_key_path, id_path);

				txlog->commit(tx);
			}
			else
			{
				auto c = d->add_container(container_name, partition_key_path, id_path);
			}

			return ok_response();
		}
		else return error_response("Database not found: " + database_name);
	}	
	else return error_response("Data server not found: " + data_server_name);
}

json server::handle_get_data_server(const json& message)
{
	std::string data_server_name;
	json v = validate_required_key(message, key_data_server_name, data_server_name);
	if (v.is_null() == false)
		return v;

	auto s = data_servers.find(data_server_name);
	if (s != data_servers.end())
	{		
		return ok_response();
	}
	else
	{
		return error_response("Data server not found: " + data_server_name);
	}
}

json server::handle_get_database(const json& message)
{
	std::string data_server_name;
	json v = validate_required_key(message, key_data_server_name, data_server_name);
	if (v.is_null() == false)
		return v;

	std::string database_name;
	v = validate_required_key(message, key_database_name, database_name);
	if (v.is_null() == false)
		return v;

	auto s = data_servers.find(data_server_name);
	if (s != data_servers.end())
	{
		auto db = s->second->get_database(database_name);
		if(db != nullptr)		
			return ok_response();
		return error_response("Database not found: " + database_name);
	}
	
	return error_response("Data server not found: " + data_server_name);
}

json server::handle_get_container(const json& message)
{
	std::string data_server_name, database_name, container_name;

	json v = get_container_path_parts(message, data_server_name, database_name, container_name);
	if (v.is_null() == false)
		return v;

	std::shared_ptr <data::server> s;
	auto c = get_container(data_server_name, database_name, container_name, s);
	if (c == nullptr)
		return error_response("Container not found.");

	json r = json::object();
	r["e"] = nullptr;
	r["pkp"] = c->get_partition_key_path();
	r["idp"] = c->get_id_path();
	return r;
}

std::shared_ptr<data::container> server::get_container(std::string data_server_name, std::string database_name, std::string container_name,
	std::shared_ptr<data::server>& server_out)
{
	server_out = nullptr;
	auto s = data_servers.find(data_server_name);
	if (s != data_servers.end())
	{
		server_out = s->second;
		auto d = s->second->get_database(database_name);
		if (d != nullptr)
			return d->get_container(container_name);		
	}
	
	return nullptr;
}

json server::handle_set_record(const json& message, bool log)
{
	std::string data_server_name, database_name, container_name;

	json v = get_container_path_parts(message, data_server_name, database_name, container_name);
	if (v.is_null() == false)
		return v;

	std::shared_ptr<data::server> s;

	auto c = get_container(data_server_name, database_name, container_name, s);
	if (c == nullptr)
		return error_response("Container not found.");
	
	if (message.contains("j") == false)
		return missing_argument_response("j");

	json j = json::parse(message["j"].get<std::string>());	
	
	if (log)
	{		
		auto txlog = s->get_txlog();
		auto tx = txlog->begin();
		txlog->write(tx, message);

		c->set_record(j);

		txlog->commit(tx);
	}
	else c->set_record(j);

	return ok_response();
}

json server::handle_get_record(const json& message)
{
	std::string data_server_name, database_name, container_name;

	json v = get_container_path_parts(message, data_server_name, database_name, container_name);
	if (v.is_null() == false)
		return v;

	std::shared_ptr <data::server> s;
	auto c = get_container(data_server_name, database_name, container_name, s);
	if (c == nullptr)
		return error_response("Container not found.");
		
	if (message.contains("pk") == false)
		return missing_argument_response("pk");
	if (message.contains("id") == false)
		return missing_argument_response("id");
	

	auto j = c->get_record(message["pk"], message["id"]);	

	json r = json::object();
	r["e"] = nullptr;
	if (j.length() == 0)
		r["j"] = nullptr;
	else
		r["j"] = j;
	return r;
}


json server::handle_delete_records(const json& message, bool log)
{
	std::string data_server_name, database_name, container_name;

	json v = get_container_path_parts(message, data_server_name, database_name, container_name);
	if (v.is_null() == false)
		return v;

	std::shared_ptr<data::server> s;
	auto c = get_container(data_server_name, database_name, container_name, s);
	if (c == nullptr)
		return error_response("Container not found.");

	std::string partition_key;
	std::string id;

	if (message.contains("pk") && message["pk"].is_null() == false)
		partition_key = message["pk"];
	if (message.contains("id") && message["id"].is_null() == false)
		id = message["id"];
	
	uint64_t tx = 0;
	auto txlog = s->get_txlog();
	if (log)
	{
		tx = txlog->begin();
		txlog->write(tx, message);
	}

	if (partition_key.length() != 0 && id.length() != 0)
		c->delete_record(partition_key, id);
	else if (partition_key.length() != 0 && id.length() == 0)
		c->delete_records(partition_key);
	else if (partition_key.length() == 0 && id.length() == 0)
		c->delete_records();

	if(log)
		txlog->commit(tx);

	return ok_response();
}

json server::handle_add_index(const json& message, bool log)
{
	std::string data_server_name, database_name, container_name;

	json v = get_container_path_parts(message, data_server_name, database_name, container_name);
	if (v.is_null() == false)
		return v;

	std::shared_ptr<data::server> s;
	auto c = get_container(data_server_name, database_name, container_name, s);
	if (c == nullptr)
		return error_response("Container not found.");

	if (message.contains("ixp") == false)
		return missing_argument_response("ixp");
	
	if (log)
	{
		auto txlog = s->get_txlog();
		auto tx = txlog->begin();
		txlog->write(tx, message);

		c->add_index(message["ixp"]);

		txlog->commit(tx);
	}
	else c->add_index(message["ixp"]);

	return ok_response();
}

std::experimental::generator<std::string> server::handle_get_records(const json& message)
{
	std::string data_server_name, database_name, container_name;

	json v = get_container_path_parts(message, data_server_name, database_name, container_name);
	if (v.is_null() == false)
		return;

	std::shared_ptr <data::server> s;
	auto c = get_container(data_server_name, database_name, container_name, s);
	if (c == nullptr)
		return;

	std::string partition_key;
	std::string index_path;
	json begin_value = nullptr;
	json end_value = nullptr;
	json query = nullptr;
	data::filter filter;
	bool apply_filter = false;

	if (message.contains("pk") && message["pk"].is_null() == false)
		partition_key = message["pk"];
	if (message.contains("ixp") && message["ixp"].is_null() == false)
		index_path = message["ixp"];
	if (message.contains("bv") && message["bv"].is_null() == false)
		begin_value = message["bv"];
	if (message.contains("ev") && message["ev"].is_null() == false)
		end_value = message["ev"];
	if (message.contains("q") && message["q"].is_null() == false)
		query = message["q"];

	if (query.is_null() == false && query.contains("f"))
	{
		try
		{	
			std::string fe;
			if (filter.parse(query, fe) == false)
			{
				console_error_line(config->get_server_name(), "Filter Parse Error", fe);				
				return;
			}

			apply_filter = true;
		}
		catch (std::exception & x)
		{
			console_error_line(config->get_server_name(), "Filter Parse Exception", x.what());			
			return;
		}
		catch (...)
		{
			console_error_line(config->get_server_name(), "Filter Parse Exception", "Unknown");			
			return;
		}
	}

	if (partition_key.length() == 0 && index_path.length() == 0 && begin_value.is_null() && end_value.is_null())
	{
		if (apply_filter)
		{
			for (auto r : c->get_records())
			{
				auto jr = json::parse(r);
				if (filter.evaluate(jr))
					co_yield r;
			}
		}
		else
		{
			for (auto r : c->get_records())
				co_yield r;
		}
	}
	else if (partition_key.length() != 0 && index_path.length() == 0 && begin_value.is_null() && end_value.is_null())
	{
		if (apply_filter)
		{
			for (auto r : c->get_records(partition_key))
			{
				auto jr = json::parse(r);
				if (filter.evaluate(jr))
					co_yield r;
			}
		}
		else
		{
			for (auto r : c->get_records(partition_key))
				co_yield r;
		}
	}
	else if (partition_key.length() == 0 && index_path.length() != 0 && begin_value.is_null() == false && end_value.is_null())
	{
		if (apply_filter)
		{
			for (auto r : c->get_records(index_path, begin_value))
			{
				auto jr = json::parse(r);
				if (filter.evaluate(jr))
					co_yield r;
			}
		}
		else
		{
			for (auto r : c->get_records(index_path, begin_value))
				co_yield r;
		}
	}
	else if (partition_key.length() == 0 && index_path.length() != 0 && begin_value.is_null() == false && end_value.is_null() == false)
	{
		if (apply_filter)
		{
			for (auto r : c->get_records(index_path, begin_value, end_value))
			{
				auto jr = json::parse(r);
				if (filter.evaluate(jr))
					co_yield r;
			}
		}
		else
		{
			for (auto r : c->get_records(index_path, begin_value, end_value))
				co_yield r;
		}
	}
	else if (partition_key.length() != 0 && index_path.length() != 0 && begin_value.is_null() == false && end_value.is_null())
	{
		if (apply_filter)
		{
			for (auto r : c->get_records(partition_key, index_path, begin_value))
			{
				auto jr = json::parse(r);
				if (filter.evaluate(jr))
					co_yield r;
			}
		}
		else
		{
			for (auto r : c->get_records(partition_key, index_path, begin_value))
				co_yield r;
		}
	}
	else if (partition_key.length() != 0 && index_path.length() != 0 && begin_value.is_null() == false && end_value.is_null() == false)
	{
		if (apply_filter)
		{
			for (auto r : c->get_records(partition_key, index_path, begin_value, end_value))
			{
				auto jr = json::parse(r);
				if (filter.evaluate(jr))
					co_yield r;
			}
		}
		else
		{
			for (auto r : c->get_records(partition_key, index_path, begin_value, end_value))
				co_yield r;
		}
	}	
}

IAsyncAction server::handle_connection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
{
	try
	{
		auto socket{ args.Socket() };
		DataReader dataReader{ socket.InputStream() };
		DataWriter dataWriter{ socket.OutputStream() };

		auto tcp_send_size_bytes = config->get_tcp_send_size_bytes();

		while (true)
		{			
			unsigned int bytesLoaded = co_await dataReader.LoadAsync(sizeof(uint32_t));
			unsigned int stringLength = dataReader.ReadUInt32();
			bytesLoaded = co_await dataReader.LoadAsync(stringLength);
			winrt::hstring request = dataReader.ReadString(bytesLoaded);

			std::string jrequest = winrt::to_string(request);

			auto j = json::parse(jrequest);

			if (j.is_null())
				co_return;

			json json_response;

			if (j.contains(key_request_type) == false)
			{
				json_response = missing_argument_response(key_request_type);
			}
			else
			{
				uint32_t request_type = j[key_request_type];

				switch ((request_types)request_type)
				{
				case request_types::add_data_server:
					json_response = handle_add_data_server(j);
					break;
				case request_types::add_database:
					json_response = handle_add_database(j, true);
					break;
				case request_types::add_container:
					json_response = handle_add_container(j, true);
					break;
				case request_types::set_record:
					json_response = handle_set_record(j, true);
					break;
				case request_types::delete_records:
					json_response = handle_delete_records(j, true);
					break;
				case request_types::add_index:
					json_response = handle_add_index(j, true);
					break;
				case request_types::get_record:
					json_response = handle_get_record(j);
					break;
				case request_types::get_records:
				{
					uint32_t store_size = 0;
					for (auto r : handle_get_records(j))
					{
						winrt::hstring sr = winrt::to_hstring(r);
						auto length = sr.size();
						if (length == 0)
							break;

						if (store_size != 0 && ((uint64_t)store_size + length + sizeof(uint32_t)) >= tcp_send_size_bytes)
						{
							store_size = 0;
							co_await dataWriter.StoreAsync();
						}

						dataWriter.WriteUInt32((uint32_t)length);
						dataWriter.WriteString(sr);

						store_size += length + sizeof(uint32_t);
					}

					dataWriter.WriteUInt32(0);
					co_await dataWriter.StoreAsync();
					continue;
				}
					break;
				case request_types::get_data_server:
					json_response = handle_get_data_server(j);
					break;
				case request_types::get_database:
					json_response = handle_get_database(j);
					break;
				case request_types::get_container:
					json_response = handle_get_container(j);
					break;
				}				
			}

			winrt::hstring s_response = winrt::to_hstring(json_response.dump());
			
			dataWriter.WriteUInt32((uint32_t)s_response.size());
			dataWriter.WriteString(s_response);
			co_await dataWriter.StoreAsync();			
		}
	}
	catch (winrt::hresult_error const& ex)
	{
		SocketErrorStatus error_status{ Windows::Networking::Sockets::SocketError::GetStatus(ex.to_abi()) };

		console_error_line(config->get_server_name(), "Socket Receive Error", get_socket_error_string(error_status));		
	}
	catch (std::exception & x)
	{
		console_error_line(config->get_server_name(), "Error", x.what());
	}
	catch (...)
	{
		console_error_line(config->get_server_name(), "Socket Receive Error", "Unknown");
	}
}

IAsyncAction server::on_connection_received_async(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
{
	auto remoteAddress = args.Socket().Information().RemoteAddress().DisplayName();
	auto remotePort = args.Socket().Information().RemotePort();
	
	console_line(config->get_server_name(), "Connection received - " + winrt::to_string(remoteAddress) + ", Port " + winrt::to_string(remotePort));	

	co_await handle_connection(sender, args);	
}
