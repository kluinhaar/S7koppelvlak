using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;

public class myS7class
{
    static libnodave.daveOSserialType fds;
    static libnodave.daveInterface di;
    static libnodave.daveConnection dc;

    public static int Main(string[] args)
    {

        int i;
        int rack, slot;
        int res;
        int readDB, readstart, readnumberofbytes;
        int writeDB, writestart, writenumberofbytes;
        int area = 0;

        var writebuffer = new byte[] { 	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        var readbuffer = new byte[] { 	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        string PLC = "255.255.255.255";

        libnodave.daveSetDebug(libnodave.daveDebugRawRead);

        if (args.Length != 9)
        {
            Console.WriteLine("");
            Console.WriteLine("Usage:");
            Console.WriteLine("./S7koppelvlak [ipaddress] [rack] [slot] [readDB] [startbyte] [numberofbytes] [writeDB] [startbyte] [numberofbytes]");
            Console.WriteLine("");
            Console.WriteLine("Examples:");
            Console.WriteLine("./S7koppelvlak 172.24.40.191 0 3 DB10 0 16 DB20 17 16");
            Console.WriteLine("");
            Console.WriteLine("./S7koppelvlak 172.24.40.191 0 3 DB100 128 32 DB100 0 64");
            Console.WriteLine("");
            return -4;
        }

	for(i=0;i<9;i++)
            Console.WriteLine("" + args[i]);

        PLC = args[0];
        rack = Convert.ToInt32(args[1]);
        slot = Convert.ToInt32(args[2]);

        readDB = Convert.ToInt32(args[3]);
        readstart = Convert.ToInt32(args[4]);
        readnumberofbytes = Convert.ToInt32(args[5]);

        writeDB = Convert.ToInt32(args[6]);
        writestart = Convert.ToInt32(args[7]);
        writenumberofbytes = Convert.ToInt32(args[8]);

	Console.WriteLine("Connection PLC " + PLC + ", rack " + Convert.ToString(rack) + ", slot " + Convert.ToString(slot) );
	Console.WriteLine("Read block  : DB" + Convert.ToString(readDB) + "[" + Convert.ToString(readstart) + ".." +  Convert.ToString(readstart + readnumberofbytes - 1) + "]");
	Console.WriteLine("Write block : DB" + Convert.ToString(writeDB) + "[" + Convert.ToString(writestart) + ".." +  Convert.ToString(writestart + writenumberofbytes - 1) + "]");

        area = libnodave.daveDB;
        fds.rfd = libnodave.openSocket(102, PLC);
        fds.wfd = fds.rfd;
        if (fds.rfd <= 0){
            return -5;
        }
        else{
            di = new libnodave.daveInterface(fds, "IF1", 0, libnodave.daveProtoISOTCP, libnodave.daveSpeed187k);
            di.setTimeout(1000000);
            res = di.initAdapter();
            if (res != 0){
                return -4;
            }
            else{
                dc = new libnodave.daveConnection(di, 0, rack, slot);
                if (dc.connectPLC() != 0){
                    Console.WriteLine("Couldn't open TCP connaction to " + args[0]);
                    return -2;
                    }
                else{
                    while (true){
                        // Incoming data
                        res = dc.readBytes(area, readDB, readstart, readnumberofbytes, null);
                        if (res == 0){
                            for (i = 0; i < readnumberofbytes; i++) {
                                readbuffer[i] = (System.Byte)dc.getU8();
                            }
                        }

			// Copy incoming data to PiFace Digital board outputs...
			Console.WriteLine(readbuffer);
			// Copy inputs form PiFace Digital board to writebuffer...

                        // Outgoing data
                        for (i = 0; i < writenumberofbytes; i++)
                            writebuffer[i] = (byte)i;
                        res = dc.writeBytes(area, writeDB, writestart, writenumberofbytes, writebuffer);
                        Thread.Sleep(1000);
                    }
                    dc.disconnectPLC();
                }
                di.disconnectAdapter();
                libnodave.closeSocket(fds.rfd);
                return 0;
            }
        }
    }
}

