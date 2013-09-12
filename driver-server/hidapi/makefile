# ---- File locations ----

MAINDIR	= $(dir $(firstword $(MAKEFILE_LIST)))
INCDIR	= $(MAINDIR)
BINDIR	= $(MAINDIR)/bin
# SRCDIR is configured below
TARGET	= $(BINDIR)/hidapi.so
# SRCFILES is configured below

# ---- Compiler Settings ----

CC		= gcc
DEBUG	= -g
COPTS	= -fPIC -O3 -c $(DEBUG)
CFLAGS	= -Wall -Iinclude -I $(INCDIR) $(COPTS)
LFLAGS	= -Wall -stdlib

# ---- Platform Specific Options ----

ifeq ($(OS),Windows_NT)
	# you're boned
	@echo "you're boned"

	SRCDIR		= $(MAINDIR)/windows
	SRCFILES	= $(SRCDIR)/hid.c
else
	UNAME_S := $(shell uname -s)
	ifeq ($(UNAME_S),Darwin)
		# produce fat binary on OS X
		COPTS		+= -arch x86_64 -arch i386
		LFLAGS		+= -arch x86_64 -arch i386 -dynamiclib -undefined suppress -flat_namespace
		LFLAGS		+= -framework CoreFoundation -framework IOKit

		SRCDIR		= $(MAINDIR)/mac
		SRCFILES	= $(SRCDIR)/hid.c
	else
		# Linux experimental settings
		LFLAGS		+= -shared -lusb -liconv

		SRCDIR		= $(MAINDIR)/linux
		SRCFILES	= $(SRCDIR)/hid-libusb.c
	endif
endif

# ---- Rules ----

OBJFILES	= $(SRCFILES:.c=.o)

# default is to build library
all: lib

# link all object files into target library
lib: $(OBJFILES)
	$(CC) -o $(TARGET) $(^) $(LFLAGS)

# compile each object file
%.o: %.c
	$(CC) $(CFLAGS) -c $<
	mv *.o $(SRCDIR)

# delete any binary files
clean:
	- rm $(OBJFILES)
	- rm $(TARGET)
