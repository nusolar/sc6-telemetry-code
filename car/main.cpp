//
//  main.cpp
//  Telepo
//
//  Created by Alex Chandel on 10/28/12.
//  Copyright (c) 2012 Alex Chandel. All rights reserved.
//

#include <iostream>
#include <sstream>

#include "Packet.h"
#include "Sequel.h"
#include "Platform.h"

using namespace std;

void consumePacket(Packet &p) {
	static Sequel db;
	db.addPacket(p);
}

int main(int argc, const char * argv[])
{
	Packet ex = Packet(7, 1111111.1);
	consumePacket(ex);
	consumePacket(ex);
	
	cout << "Hello, World!\n";
	
    return 0;
}