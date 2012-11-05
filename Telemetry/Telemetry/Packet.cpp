//
//  Packet.cpp
//  Telemetry
//
//  Created by Alex Chandel on 11/4/12.
//  Copyright (c) 2012 Alex Chandel. All rights reserved.
//

#include "Packet.h"

char *Packet::serialize() {
	bytes = new char[sizeof(CAN_ID) + sizeof(data)]();
	bytes[0] = CAN_ID;
	bytes[sizeof(CAN_ID)] = data;
	return bytes;
}

Packet::~Packet() {
	delete[] bytes;
}