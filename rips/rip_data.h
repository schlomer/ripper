/**************************************
  Rip Server (C) 2019 Shannon Schlomer
 **************************************/

#ifndef RIP_DATA_H
#define RIP_DATA_H

#include <string>
#include <memory>
#include <unordered_map>
#include <map>
#include <ostream>
#include <istream>
#include <mutex>
#include <shared_mutex>
#include <future>
#include <experimental/generator>
#include <fstream>
#include <atomic>
#include "json.hpp"
using namespace nlohmann;

namespace rip
{
	namespace data
	{	
		class spin_lock {
			std::atomic_flag locked = ATOMIC_FLAG_INIT;
		public:
			void lock() {
				while (locked.test_and_set(std::memory_order_acquire)) { ; }
			}
			void unlock() {
				locked.clear(std::memory_order_release);
			}
		};

		class spin_lock_guard
		{
			spin_lock& spin;
		public:
			spin_lock_guard(spin_lock& spin) : spin(spin) { spin.lock(); }
			~spin_lock_guard() { spin.unlock(); }
			
		};

		// transaction_log
		class transaction_log
		{
			std::string path;
			std::atomic<uint64_t> next_id;
			std::ofstream file;
			spin_lock spin;
			
			void close();
			void open();

		public:
			transaction_log(std::string path);
			uint64_t begin();
			void write(uint64_t id, const json& j);
			void commit(uint64_t id);
			std::experimental::generator<json> get_logs();
			void set_next_id(uint64_t id);
		};

		// fop
		class fop
		{
		public:
			virtual json evaluate() const = 0;			
		};

		class fop_value : public fop
		{
			json v;
		public:
			fop_value(const json& v) : v(v) {}

			json evaluate() const { return v; }
		};

		class fop_parameter : public fop
		{
			std::string path;
			const json* j;

		public:
			fop_parameter(std::string path) : path(path), j(nullptr) {}
			void set_record(const json* _j) { this->j = _j; }

			json evaluate() const
			{
				return j->at(json::json_pointer(path));
			}
		};

		class fop_command : public fop
		{
			std::shared_ptr<fop> a;
			std::shared_ptr<fop> b;
			std::string op;
			static bool ends_with(const std::string& str, const std::string& suffix)
			{
				return str.size() >= suffix.size() && 0 == str.compare(str.size() - suffix.size(), suffix.size(), suffix);
			}

			static bool starts_with(const std::string& str, const std::string& prefix)
			{
				return str.size() >= prefix.size() && 0 == str.compare(0, prefix.size(), prefix);
			}

			void to_lower(std::string& str) const
			{
				for (auto& c : str) c = (char)std::tolower(c);
			}

		public:
			fop_command(std::string op, const std::shared_ptr<fop>& a, const std::shared_ptr<fop>& b) : op(op), a(a), b(b) {}
			json evaluate() const
			{	
				try
				{
					if (op == "=")
						return a->evaluate() == b->evaluate();
					if (op == "<>")
						return a->evaluate() != b->evaluate();
					if (op == ">=")
						return a->evaluate() >= b->evaluate();
					if (op == "<=")
						return a->evaluate() <= b->evaluate();
					if (op == "<")
						return a->evaluate() < b->evaluate();
					if (op == ">")
						return a->evaluate() > b->evaluate();
					if (op == "=*" || op == "*=*" || op == "*=")
					{
						auto av = a->evaluate();
						if (av.is_string() == false)
							return false;
						auto bv = b->evaluate();
						if (bv.is_string() == false)
							return false;
						std::string sa = av;
						std::string sb = bv;						
						to_lower(sa);
						to_lower(sb);
						if (op == "=*")
							return starts_with(sa, sb);
						if (op == "*=*")
							return sa.find(sb) != std::string::npos;
						if (op == "*=")
							return ends_with(sa, sb);
					}
					if (op == "and")
					{
						bool av = a->evaluate();
						bool ab = b->evaluate();
						return av && ab;
					}
					if (op == "or")
					{
						bool av = a->evaluate();
						bool ab = b->evaluate();
						return av || ab;
					}
				}
				catch (std::exception&)
				{
					return false;
				}
				catch (...)
				{
					return false;
				}

				return false;
			}
		};

		class filter
		{
			std::unordered_map<std::string, std::shared_ptr<fop_parameter>> parameters;
			std::shared_ptr<fop> root;
			
			std::shared_ptr<fop> parse_filter(const json& filter_source, std::string& error_out);
			bool parse_path_value(std::string path, const json& value, std::shared_ptr<fop>& path_out, std::shared_ptr<fop>& value_out, std::string& error_out);
			std::shared_ptr<fop_parameter> get_or_add_parameter(std::string path);

		public:			
			bool parse(const json& source, std::string& error_out);
			bool evaluate(const json& record) const;
		};

		// container
		class container
		{
			class partition
			{
				friend container;				
				
				mutable std::shared_mutex mtx_records;				
				std::unordered_map<std::string, std::string> records;

				std::unordered_map<std::string, std::shared_ptr<std::multimap<json, std::string>>> indexes;
				std::unordered_map<std::string, std::shared_ptr<std::unordered_map<std::string, json>>> rev_indexes;			
				
				void unsafe_set_record(std::string id, const json& data);
				std::string get_record(std::string id) const;
				void delete_record(std::string id, bool& partition_is_empty);
				void delete_records();

				void unsafe_add_indexes(std::vector<std::string> paths);
				void unsafe_update_indexes(std::string id, const json& value);
				void unsafe_delete_from_indexes(std::string id);			
			};

			mutable std::shared_mutex mtx_partitions;
			std::unordered_map<std::string, std::shared_ptr<partition>> partitions;

			std::string name;
			std::string partition_key_path;
			std::string id_path;
			std::vector<std::string> index_paths;			
			
		public:
			container(std::string name, std::string partition_key_path, std::string id_path);	

			void set_record(const json& value);			
			std::string get_record(std::string partition_key, std::string id) const;
			void delete_record(std::string partition_key, std::string id);
			void delete_records(std::string partition_key);
			void delete_records();
			std::string get_name() const;
			std::string get_partition_key_path() const;
			std::string get_id_path() const;

			std::experimental::generator<std::string> get_records() const;
			std::experimental::generator<std::string> get_records(std::string partition_key) const;
			std::experimental::generator<std::string> get_records(std::string partition_key, std::string index_path, const json& value) const;
			std::experimental::generator<std::string> get_records(std::string partition_key, std::string index_path, const json& begin_value, const json& end_value) const;
			std::experimental::generator<std::string> get_records(std::string index_path, const json& value) const;
			std::experimental::generator<std::string> get_records(std::string index_path, const json& begin_value, const json& end_value) const;

			void add_index(std::string path);
		};

		// database
		class database
		{
			mutable std::mutex mtx_containers;
			std::string name;
			std::unordered_map<std::string, std::shared_ptr<container>> containers;
		public:
			database(std::string name);
			std::shared_ptr<container> add_container(std::string name, std::string partition_key_path, std::string id_path);
			std::shared_ptr<container> get_container(std::string name) const;
			std::string get_name() const;
			std::vector<std::shared_ptr<container>> get_containers() const;
		};

		// server
		class server
		{
			mutable std::mutex mtx_databases;
			std::unordered_map<std::string, std::shared_ptr<database>> databases;
			std::string name;
			std::string txlog_path;
			std::shared_ptr<transaction_log> txlog;
		public:
			server(std::string name, std::string txlog_path);
			std::string get_name() const;
			std::shared_ptr<database> add_database(std::string name);
			std::shared_ptr<database> get_database(std::string name) const;	
			std::vector<std::shared_ptr<database>> get_databases() const;
			std::string get_txlog_path() const;
			std::shared_ptr<transaction_log> get_txlog();
		};
	}
}

#endif