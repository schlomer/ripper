/**************************************
  Rip Server (C) 2019 Shannon Schlomer
 **************************************/

#ifndef RIP_TYPES_H
#define RIP_TYPES_H

#include <iostream>
#include <string>

namespace rip
{
	inline void console_line()
	{
		std::cout << std::endl;
	}

	inline void console_line(std::string message)
	{
		std::cout << message << std::endl;
	}

	inline void console_error_line(std::string error_type, std::string message)
	{
		std::cout << error_type << " - " << message << std::endl;
	}

	inline void console_line(std::string server_name, std::string message)
	{
		std::cout << server_name << " : " << message << std::endl;
	}

	inline void console_error_line(std::string server_name, std::string error_type, std::string message)
	{
		std::cout << server_name << " : " << error_type << " - " << message << std::endl;
	}
	
}

#endif