LIBPATHS	= /usr/local/lib

CPP			= g++
RM			= rm -f

DFLAGS		= -g
CPPFLAGS	= $(DFLAGS) -std=c++11 -Wall -Wextra
LDFLAGS		= -Wl,-rpath $(foreach libpath,$(LIBPATHS),-Wl,$(libpath))
LDLIBS		= -lsqlite3 -lzmq -L$(LIBPATHS)
INC			= -I./easySQLite -I/usr/local/include

SRCS		= $(wildcard **/*.cpp) $(wildcard *.cpp)
OBJS		= $(subst .cpp,.o,$(SRCS))

all: telem


telem: $(OBJS)
	$(CPP) $(LDFLAGS) -o $@ $(OBJS) $(LDLIBS)

%.o: %.cpp
	$(CPP) -c $(CPPFLAGS) $(INC) $*.cpp -o $*.o

clean:
	$(RM) $(OBJS)

dist-clean: clean
	$(RM) telem
