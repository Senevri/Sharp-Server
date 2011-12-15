SRCS = Program.cs
OUTFILE = EchoSrv.exe

.phony: all clean
all: 
	gmcs $(SRCS) -out:$(OUTFILE)

clean: 
	-rm -rf $(OUTFILE)
