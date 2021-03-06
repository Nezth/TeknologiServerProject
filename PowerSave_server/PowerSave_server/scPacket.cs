﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerSave_server
{
    /* 
     *  "SC_" is a server / client packet, so it can be sent by both
     *  "C_" is a client packet
     *  "S_" is a server packet
     */
    enum PACKET_TYPE : byte
    {
        SC_PING = 0x00,
        C_HANDSHAKE = 0x01,
        S_ACCEPT = 0x02,
        C_GET_ONLINE_CLIENTS = 0x03,
        S_SEND_ONLINE_CLIENTS = 0x04,
        C_UPDATE_SOCKET_STATE = 0x05,
        S_SOCKET_UPDATE = 0x06,
        C_SOCKET_POWER_UPDATE = 0x07,
        S_SOCKET_POWER_UPDATE = 0x08,
        C_REQUEST_SOCKET_INFO = 0x09,
        S_SOCKET_POWER_INFO = 0x0A,
        SC_SEND_XML = 0x0B,
        C_GET_XML = 0x0C,
        SC_SEND_PICTURE = 0x0D,
        C_GET_PICTURE = 0x0E,
        // invalid states
        ERR_NOT_DONE = 0xFC,
        ERR_INVALID_PACKET = 0xFE,
        ERR_NONE = 0xFF,
    };
    // server client packet
    class scPacket
    {
        private byte m_packetID;
        List<byte> m_packetData;
        UInt16 m_pos; // used so we can read and write data
        public scPacket(PACKET_TYPE type)
        {
            m_packetID = (byte)type;
            m_pos = 1;
            m_packetData = new List<byte>();
        }

        public scPacket()
        {
            m_packetID = (byte)PACKET_TYPE.ERR_NONE;
            m_pos = 0;
            m_packetData = new List<byte>();
        }

        public byte getPacketType()
        {
            return m_packetID;
        }
        /*
         * returns true if we got a valid packet
         * returns false if we couldn't parse the packet
         * m_packetID is always set to a value
         */
        public bool parseRawData(List<byte> raw)
        {
            if (raw.Count == 0)
            {
                m_packetID = (byte)PACKET_TYPE.ERR_NOT_DONE;
                return false;
            }
            switch((PACKET_TYPE)raw[0])
            {
                case PACKET_TYPE.C_GET_ONLINE_CLIENTS:
                    // it's only 1 byte long
                    m_packetID = raw[0];
                    m_pos = 0;
                    return true;
                case PACKET_TYPE.C_HANDSHAKE:
                    // it's only 1 byte long anyways
                    m_packetID = raw[0];
                    m_pos = 0;
                    return true;
                case PACKET_TYPE.C_UPDATE_SOCKET_STATE:
                    if (raw.Count >= 4)
                    {
                        m_packetID = raw[0];
                        for (int i = 1; i < 4; i++)
                            m_packetData.Add(raw[i]);
                        m_pos = 0;
                        return true;
                    }
                    m_packetID = (byte)PACKET_TYPE.ERR_NOT_DONE;
                    return false;
                case PACKET_TYPE.C_SOCKET_POWER_UPDATE:
                    if (raw.Count >= 7)
                    {
                        m_packetID = raw[0];
                        for (int i = 1; i < 7; i++)
                            m_packetData.Add(raw[i]);
                        m_pos = 0;
                        return true;
                    }
                    m_packetID = (byte)PACKET_TYPE.ERR_NOT_DONE;
                    return false;
                case PACKET_TYPE.C_REQUEST_SOCKET_INFO:
                    if (raw.Count >= 3)
                    {
                        m_packetID = raw[0];
                        for (int i = 1; i < 3; i++)
                            m_packetData.Add(raw[i]);
                        m_pos = 0;
                        return true;
                    }
                    m_packetID = (byte)PACKET_TYPE.ERR_NOT_DONE;
                    return false;
                case PACKET_TYPE.SC_PING:
                    if (raw.Count >= 3)
                    {
                        short len = (short)(raw[1] << 8 | raw[2]);
                        if (raw.Count >= 3 + len*2)
                        {
                            for (int i = 1; i < 3 + len*2; i++)
                                m_packetData.Add(raw[i]);
                            m_pos = 0;
                            m_packetID = raw[0];
                            return true;
                        }
                    }
                    m_packetID = (byte)PACKET_TYPE.ERR_NOT_DONE;
                    return false;
                case PACKET_TYPE.SC_SEND_XML:
                    m_packetID = (byte)PACKET_TYPE.ERR_NOT_DONE;
                    if (raw.Count < 3)
                        return false;
                    short strlen = (short)(raw[1] << 8 | raw[2]);
                    if (raw.Count < 3 + strlen * 2)
                        return false;
                    for (int i = 1; i < 3 + strlen * 2; i++)
                        m_packetData.Add(raw[i]);
                    m_packetID = raw[0];
                    m_pos = 0;
                    return true;
                case PACKET_TYPE.C_GET_XML:
                    m_packetID = raw[0];
                    m_pos = 0;
                    return true;
                case PACKET_TYPE.SC_SEND_PICTURE:
                    m_packetID = (byte)PACKET_TYPE.ERR_NOT_DONE;
                    if (raw.Count < 7)
                        return false;
                    strlen = (short)(raw[1] << 8 | raw[2]);
                    if (raw.Count < 7 + strlen * 2)
                        return false;
                    int offset = 3 + strlen * 2;
                    int dataLenght = raw[offset] << 24 |
                                     raw[offset + 1] << 16 |
                                     raw[offset + 2] << 8 |
                                     raw[offset + 3];
                    if (raw.Count < 7 + strlen * 2 + dataLenght)
                        return false;
                    m_pos = 0;
                    m_packetID = raw[0];
                    for (int i = 1; i < 7 + strlen * 2 + dataLenght; i++ )
                        m_packetData.Add(raw[i]);
                    return true;
                case PACKET_TYPE.C_GET_PICTURE:
                    m_packetID = (byte)PACKET_TYPE.ERR_NOT_DONE;
                    if (raw.Count < 3)
                        return false;
                    strlen = (short)(raw[1] << 8 | raw[2]);
                    if (raw.Count < 3 + strlen * 2)
                        return false;
                    m_pos = 0;
                    m_packetID = raw[0];
                    for(int i = 1; i < 3 + strlen*2; i++)
                        m_packetData.Add(raw[i]);
                    return true;
            }
            m_packetID = (byte)PACKET_TYPE.ERR_INVALID_PACKET;
            return false;
        }

        public int getSize()
        {
            return m_packetData.Count + 1;
        }

        public List<byte> getRawData()
        {
            List<byte> tmp = new List<byte>();
            tmp.Add(m_packetID);
            for (int i = 0; i < m_packetData.Count; i++)
                tmp.Add(m_packetData[i]);
            return tmp;
        }

        public void writeShort(short data)
        {
            byte first = (byte)(data >> 8),
                 second = (byte)(data);
            m_packetData.Add(first);
            m_packetData.Add(second);
        }

        public void writeByte(byte data)
        {
            m_packetData.Add(data);
        }

        public void writeLong(int data)
        {
            writeShort((short)(data >> 16));
            writeShort((short)data);
        }

        public void writeString(string msg)
        {
            writeShort((short)msg.Length);
            for (int i = 0; i < msg.Length; i++)
            {
                writeShort((short)msg[i]);
            }
        }

        public byte readByte()
        {
            return m_packetData[m_pos++];
        }

        public short readShort()
        {
            short tmp = (short)(readByte());
            tmp <<= 8;
            tmp |= (short)readByte();
            return tmp;
        }

        public int readLong()
        {
            int tmp = (int)readShort();
            tmp <<= 16;
            tmp |= readShort();
            return tmp;
        }

        public string readString()
        {
            string msg = "";
            short len = readShort();
            for (int i = 0; i < len; i++)
                msg += (char)readShort();
            return msg;
        }
    }
}
