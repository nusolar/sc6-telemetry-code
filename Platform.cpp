//
//  Platform.cpp
//  Telemetry
//
//  Created by Alex Chandel on 11/5/12.
//  Copyright (c) 2012 Alex Chandel. All rights reserved.
//

#include "Platform.h"

using namespace std;

Platform::OS Platform::os() {
	return OS_MAC;
}

string Platform::varDirectory;

string Platform::var() {
	if (varDirectory.empty()) {
		char *home = getenv("HOME");
		varDirectory.append(home);
		cout << "$HOME = " << home << endl;
		switch (os()) {
			case OS_MAC:
				varDirectory.append("/Desktop/");
				break;
			case OS_WINDOWS:
				varDirectory.append("/Desktop/");
				break;
			case OS_BEAGLE:
				varDirectory.append("/");
				break;
			default:
				varDirectory = string("/var/tmp/");
				break;
		}
	}
	return varDirectory;
}