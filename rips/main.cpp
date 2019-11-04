#include "pch.h"

using namespace winrt;
using namespace Windows::Foundation;
using namespace std::chrono_literals;
using namespace rip;

#include <iostream>
#include <memory>

std::shared_ptr<configuration> config;

int main()
{
    init_apartment();

	std::cout << std::endl;
	std::cout << "ripper server (C) 2019 Shannon Schlomer" << std::endl;
	std::cout << std::endl;

	config = std::make_shared<configuration>();

	if (config->load("rip-config.json") == false)
	{
		console_error_line("Error", "Could not load rip-config.json");		
		return 1;
	}
	else
	{
		console_line();
		console_line("Configuration");
		console_line("  serverName: " + config->get_server_name());
		console_line("  tcpPort: " + std::to_string(config->get_tcp_port()));
		console_line("  tcpSendSizeBytes: " + std::to_string(config->get_tcp_send_size_bytes()));
		console_line("  transactionLogPath: " + config->get_transaction_log_path());
		console_line();
	}

	console_line("Starting server...");
	networking::server s(config, "ripper");

	s.start_async().get();

	char key;	
	std::cout << std::endl;
	std::cin >> key;

	return 0;
}
