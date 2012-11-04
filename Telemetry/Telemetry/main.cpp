//
//  main.cpp
//  Telepo
//
//  Created by Alex Chandel on 10/28/12.
//  Copyright (c) 2012 Alex Chandel. All rights reserved.
//

#include <iostream>

struct packet {
	int CAN_ID;
	int length;
	struct data;
};

int main(int argc, const char * argv[])
{
	std::cout << "Hello, World!\n";
    return 0;
}