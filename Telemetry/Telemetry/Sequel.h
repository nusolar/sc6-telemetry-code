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

#include "easySQLite/SqlCommon.h"
#include "easySQLite/SqlField.h"
#include "easySQLite/SqlDatabase.h"
#include "easySQLite/SqlTable.h"

#include "Packet.h"
#include "Logger.h"

class Sequel {
	static const char *dbfile;
	static sql::Field defPacket[7];
	sql::Database cxn;
public:
	long addPacket(Packet &packet);
	void packetSent(long key);
	std::vector<Packet> allocateUnsentPackets();
	//static Sequel db;
};

#endif /* defined(__Telemetry__Sequel__) */
