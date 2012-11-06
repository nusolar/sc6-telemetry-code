//
//  MsgSender.cpp
//  Telemetry
//
//  Created by Alex Chandel on 11/6/12.
//  Copyright (c) 2012 Alex Chandel. All rights reserved.
//

#include "MsgSender.h"

using namespace std;

void MsgSender::send() {
	static Sequel db;
	
	zmq::context_t context (1);
	zmq::socket_t socket (context, ZMQ_REP);
	cout << "Connecting to server...\n";
	socket.connect("tcp://localhost:5555");
	
	while (true) {
		zmq::message_t request (Packet::nbytes);
		//should append
		blob = db.unsentPackets();
		memcpy((void *)request.data(), & blob[0], Packet::nbytes);
		socket.send(request);
		
		zmq::message_t reply;
		socket.recv(&reply);
		//remove ACK'd packets
	}
	return;
}