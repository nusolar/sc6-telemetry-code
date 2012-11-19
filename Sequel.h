//
//  Sequel.h
//  Telemetry
//
//  Created by Alex Chandel on 11/4/12.
//  Copyright (c) 2012 Alex Chandel. All rights reserved.
//

#ifndef __Telemetry__Sequel__
#define __Telemetry__Sequel__

#include <vector>

#include "SqlCommon.h"
#include "SqlField.h"
#include "SqlDatabase.h"
#include "SqlTable.h"

#include "Packet.h"
#include "Logger.h"

class Sequel {
	static sql::string dbname;
	static sql::string dbfile();
	static sql::Field defPacket[6];
	sql::Database cxn;
public:
	long addPacket(Packet &packet);
	void acknowledgePacket(int64_t key);
	std::vector<Packet> unsentPackets();
	//static Sequel db;
};

#endif /* defined(__Telemetry__Sequel__) */
