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
	socket.connect("tcp://elder.chandel.net:5555");
	
	while (true) {
		blob = db.unsentPackets();
		
		zmq::message_t request (Packet::length);
		memcpy((void *)request.data(), & blob[0], Packet::length); //should append
		socket.send(request);
		
		zmq::message_t reply;
		socket.recv(&reply);
		memcpy((void *)packets, reply.data(), reply.size());
		
		for (int i = 0; i<reply.size(); i++) {
			db.acknowledgePacket(packets[i]);
		}
	}
	return;
}