//
//  Platform.h
//  Telemetry
//
//  Created by Alex Chandel on 11/5/12.
//  Copyright (c) 2012 Alex Chandel. All rights reserved.
//

#ifndef __Telemetry__Platform__
#define __Telemetry__Platform__

#include <iostream>
#include <cstdlib>
#include <string>

class Platform {
	static std::string varDirectory;
	
public:
	enum OS {OS_MAC, OS_WINDOWS, OS_BEAGLE};
	
	static OS os();
	static std::string var();
};

#endif /* defined(__Telemetry__Platform__) */
