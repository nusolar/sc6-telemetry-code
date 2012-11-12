//
//  MsgSender.h
//  Telemetry
//
//  Created by Alex Chandel on 11/6/12.
//  Copyright (c) 2012 Alex Chandel. All rights reserved.
//

#ifndef __Telemetry__MsgSender__
#define __Telemetry__MsgSender__

#include <iostream>
#include <vector>
#include "../include/zmq.hpp"

#include "Packet.h"
#include "Sequel.h"

struct MsgSender {
	void send();
	std::vector<Packet> blob;
	int64_t *packets;
};

#endif /* defined(__Telemetry__MsgSender__) */
