//
//  Logger.h
//  Telemetry
//
//  Created by Alex Chandel on 11/4/12.
//  Copyright (c) 2012 Alex Chandel. All rights reserved.
//

#ifndef __Telemetry__Logger__
#define __Telemetry__Logger__

#include <iostream>
#include <fstream>
#include <chrono>
#include "Packet.h"

struct Logger {
	static void logPacket(Packet &packet);
	static void logError(std::string message);
};

#endif /* defined(__Telemetry__Logger__) */
