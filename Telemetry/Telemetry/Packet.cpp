//
//  Packet.cpp
//  Telemetry
//
//  Created by Alex Chandel on 11/4/12.
//  Copyright (c) 2012 Alex Chandel. All rights reserved.
//

#include "Packet.h"

static std::chrono::duration<int,std::ratio<60*60*24>> y2012 (15340);

char *Packet::serialize() {
	bytes[0] = CAN_ID;
	bytes[sizeof(CAN_ID)] = data;
	bytes[sizeof(CAN_ID) + sizeof(data)] = time_ms;
	return bytes;
}

Packet::Packet(short can, double d) : CAN_ID(can), data(d) {
	using namespace std::chrono;
	time_ms = duration_cast<milliseconds>(system_clock::now().time_since_epoch()-=y2012).count();
}