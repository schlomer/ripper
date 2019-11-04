/**************************************
  Rip Server (C) 2019 Shannon Schlomer
 **************************************/

#include "pch.h"

using namespace rip;
using namespace rip::data;

std::shared_ptr<fop_parameter> filter::get_or_add_parameter(std::string path)
{
	auto i = parameters.find(path);
	if (i != parameters.end())
		return i->second;

	auto p = std::make_shared<fop_parameter>(path);
	parameters[path] = p;
	return p;
}

bool filter::parse_path_value(std::string path, const json& value, std::shared_ptr<fop>& path_out, std::shared_ptr<fop>& value_out, std::string& error_out)
{
	if (path.length() == 0)
	{
		error_out = "Path cannot be empty.";
		return false;
	}	

	if (value.is_primitive() == false || value.is_null())
	{
		error_out = "Comparison value must be a non-null primitive or path.";
		return false;
	}

	std::shared_ptr<fop> v;
		
	if (value.is_string())
	{
		std::string vpath = value;
		if (vpath.length() == 0)
		{
			error_out = "Comparison value cannot be empty.";
			return false;
		}
		if (vpath[0] == '/')
		{
			value_out = get_or_add_parameter(vpath);
		}
	}

	if (value_out == nullptr)
		value_out = std::make_shared<fop_value>(value);

	path_out = get_or_add_parameter(path);
	return true;
}

std::shared_ptr<fop> filter::parse_filter(const json& filter_source, std::string& error_out)
{	
	if (filter_source.is_object())
	{
		for (auto i = filter_source.begin(); i != filter_source.end(); ++i)
		{
			auto op = i.key();

			if (op == "and" || op == "or")
			{	
				auto a = parse_filter(i.value()[0], error_out);
				if (a == nullptr)
					return nullptr;				
				auto b = parse_filter(i.value()[1], error_out);
				if (b == nullptr)
					return nullptr;
							   
				return std::make_shared<fop_command>(op, a, b);					
			}
			else if (op == ">=" 
				|| op == "=" 
				|| op == "<="
				|| op == "<>"
				|| op == "<"
				|| op == ">"
				|| op == "=*"
				|| op == "*="
				|| op == "*=*"
				)
			{
				auto ii = i.value().begin();				
				auto path = ii.key();				
				auto value = ii.value();

				std::shared_ptr<fop> p;
				std::shared_ptr<fop> v;
				if (parse_path_value(path, value, p, v, error_out) == false)
					return nullptr;
				
				return std::make_shared<fop_command>(op, p, v);				
			}
		}
	}

	return nullptr;
}

bool filter::parse(const json& source, std::string& error_out)
{
	root = nullptr;
	parameters.clear();
	error_out = "";
	try
	{	
		if (source.contains("f") == false)
		{
			error_out = "Missing filter section.";
			return false;
		}
		
		root = parse_filter(source["f"], error_out);
		if (root == nullptr)
			return false;

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

bool filter::evaluate(const json& record) const
{
	for (auto&& i : parameters)
		i.second->set_record(&record);

	return root->evaluate();
}
