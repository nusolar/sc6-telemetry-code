#include "hidapi.h"
#include <wchar.h>

int main(int argc, char const *argv[])
{
	unsigned short vendor_id = 1027;
	unsigned short product_id = 65448;
	wchar_t *serial_number = NULL;
	hid_device *dev = hid_open(vendor_id, product_id, serial_number);

	if (hid_device != NULL)
	{
	}

	return 0;
}
