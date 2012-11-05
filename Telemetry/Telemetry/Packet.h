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
struct Packet {
	short CAN_ID = 0;
	double data = 1.0;
	
	char *bytes;
	char *serialize();
	~Packet();
};

#endif /* defined(__Telemetry__Packet__) */
