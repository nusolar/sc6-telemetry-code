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
	ofstream log (Platform::var().append("packets.log").c_str(), fstream::app);
	log << "Time: " << packet.time_ms << " ms\n";
	log << "C ID: " << packet.CAN_ID << endl;
	log << "Data: " << *(long *)&packet.data << endl << endl;
	log.close();
}

void Logger::logError(string message) {
	cout << message << endl;
}