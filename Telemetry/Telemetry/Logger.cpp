//
//  Logger.cpp
//  Telemetry
//
//  Created by Alex Chandel on 11/4/12.
//  Copyright (c) 2012 Alex Chandel. All rights reserved.
//

#include "Logger.h"

using namespace std;

void Logger::logPacket(Packet &packet) {
	using namespace std::chrono;
	ofstream log ("/Users/alex/packets.log");
	log << "Time: ";
	//log << duration_cast<milliseconds>(system_clock::now().time_since_epoch()).count() << " ms\n";
	log << "C ID: " << packet.CAN_ID << endl;
	log << "Data: " << *(long *)&packet.data << endl << endl;
	log.close();
}

void Logger::logError(string message) {
	cout << message << endl;
}