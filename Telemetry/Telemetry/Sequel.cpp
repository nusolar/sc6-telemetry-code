//
//  Sequel.cpp
//  Telemetry
//
//  Created by Alex Chandel on 11/4/12.
//  Copyright (c) 2012 Alex Chandel. All rights reserved.
//

#include "Sequel.h"

using namespace sql;

const char *Sequel::dbfile = "/Users/alex/sqlite.db";
Field Sequel::defPacket[7] = {
	Field(FIELD_KEY),
	Field("pid", type_int, flag_primary_key),
	Field("CAN_ID", type_int, flag_not_null),
	Field("data", type_float, flag_not_null),
	Field("time", type_time, flag_not_null),
	Field("sent", type_bool, flag_not_null),
	Field(DEFINITION_END)
};

long Sequel::addPacket(Packet &packet) {
	Logger::logPacket(packet);
	try {
		cxn.open(string(dbfile));
		Table tbPacket(cxn.getHandle(), "packet", defPacket);
		if (!tbPacket.exists())
			tbPacket.create();
		
		Record record(tbPacket.fields());
		record.setInteger("CAN_ID", packet.CAN_ID);
		record.setDouble("data", packet.data);
		record.setTime("time", time::now());
		record.setBool("sent", false);
		
		tbPacket.addRecord(&record);
		Value *v = record.getKeyIdValue();
		cxn.close();
		return v->asInteger();
	} catch (Exception e) {
		Logger::logError(e.msg());
	} return -1;
}

void Sequel::packetSent(long key) {
	try {
		cxn.open(string(dbfile));
		Table tbPacket(cxn.getHandle(), "packet", defPacket);
		if (!tbPacket.exists())
			Logger::logError("Warning: table doesn't exist");
		
		if (Record *record = tbPacket.getRecordByKeyId(key)) {
			record->setBool("sent", true);
		} else {
			Logger::logError("Warning: the key of the sent packet isn't in the DB");
		}
		cxn.close();
	} catch (Exception e) {
		Logger::logError(e.msg());
	}
}

std::vector<Packet> Sequel::allocateUnsentPackets() {
	std::vector<Packet> unsents;
	try {
		cxn.open(string(dbfile));
		Table tbPacket(cxn.getHandle(), "packet", defPacket);
		if (!tbPacket.exists())
			Logger::logError("Warning: table doesn't exist");
		
		Packet packet;
		for (int i = 0; i < tbPacket.totalRecordCount(); i++) {
			if (Record *r = tbPacket.getRecord(i)) {
				if (!r->getValue("sent")->asBool()) {
					packet.CAN_ID = r->getValue("CAN_ID")->asInteger();
					packet.data = r->getValue("data")->asDouble();
				}
			}
		}
	} catch (Exception e) {
		Logger::logError(e.msg());
	} return unsents;
}