//
//  Packet.h
//  Telemetry
//
//  Created by Alex Chandel on 11/4/12.
//  Copyright (c) 2012 Alex Chandel. All rights reserved.
//

#ifndef __Telemetry__Packet__
#define __Telemetry__Packet__

#include <iostream>
#include <chrono>

struct Packet {
	short CAN_ID = 0;
	double data = 1.0;
	long time_ms = 0;
	
	static const size_t nbytes = sizeof(CAN_ID) + sizeof(data) + sizeof(time_ms);
	char bytes[nbytes];
	char *serialize();
	Packet(short can = 0, double d = 1.0);
};

#endif /* defined(__Telemetry__Packet__) */
