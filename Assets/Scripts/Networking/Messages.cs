
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Messages
{
    public enum server_t
    {
        master_server,
        connection_server,
        gameplay_server
    }

    unsafe class HelpersRaw {
        public static int getMessageSize(message_t type)
        {
            /* Generate */ return -1;
        }
        public static message_t GetMessageType(byte[] data, int offset) {
            connection_m m;
            m.type = 0;
            m.bytes[0] = data[offset];
            m.bytes[1] = data[offset + 1];
            return (message_t)m.type; 
        }

        public static void fillRotation(float rotation, out UInt16 r)
        {
            r = (UInt16)(rotation/360.0f * 10000);
        }
        public static float readRotation(UInt16 r)
        {
            return (float)r*360 / 10000.0f;
        }

        const float pos_acc_factor = 10000.0f;
        public static void fillPosition(Vector3 pos, Vector3 max_bounds, out position_sm m) {            
            m.x = (Int16)(pos.x * pos_acc_factor / max_bounds.x);
            m.y = (Int16)(pos.y * pos_acc_factor / max_bounds.y);
            m.z = (Int16)(pos.z * pos_acc_factor / max_bounds.z);
        }
        public static Vector3 readPosition(position_sm m, Vector3 max_bounds) { 
            Vector3 position = new Vector3();

            position.x = m.x * max_bounds.x / pos_acc_factor;
            position.y = m.y * max_bounds.y / pos_acc_factor;
            position.z = m.z * max_bounds.z / pos_acc_factor;

            return position;
        }
        // not up to standard, will do later.
        public static void fillUsername(string s, out username_sm username) {
            char[] username_arr = s.ToCharArray();

            for (int i = 0; i < username_arr.Length; i++)
            {
                if (i >= 15) { break; } // Not supporting longer usernames
                username.user[i] = (byte)username_arr[i];
            }

            username.flags = 0;
        }
        public static string readUsername(username_sm username) {
            string s = "";
            for (int i = 0; i < 15; i++)
            {
                if (username.user[i] == 0) { break; }
                
                s += (char)username.user[i];
            }
            return s;
        }
    }
}
