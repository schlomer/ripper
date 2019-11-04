#pragma once
#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.Foundation.Collections.h>
#include <winrt/Windows.Networking.Sockets.h>
#include <winrt/Windows.Storage.Streams.h>
#include <sstream>
#include <chrono>
#include <string>
#include <atomic>
#include <memory>
#include <cstdint>
#include <vector>
#include <thread>

#include "json.hpp"

#include "rip_types.h"
#include "rip_configuration.h"
#include "rip_data.h"
#include "rip_networking.h"
