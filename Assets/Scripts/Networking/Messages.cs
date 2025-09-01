
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Messages
{
    public enum message_t {
        connection,
        connection_reply,
        movement,
        movement_reply,
        initiate_udp,
        initiate_udp_reply,
        server_registration,
        server_registration_reply,
        server_info,
        server_info_reply,
        player_registration,
        player_load_data,
        request_last_position,
        request_last_position_reply,
        player_disconnect,
        // Unimplemented
        entity_target_move_id,
        entity_target_move_vec,
        player_registration_reply,
        // End unimplemented
        racing_lobby_action,
        racing_lobby_update,

    };

    public enum server_t
    {
        master_server,
        connection_server,
        gameplay_server
    }

    unsafe class Helpers {
        public static int getMessageSize(message_t type)
        {
            switch (type)
            {
                case message_t.connection:
                    return connection_m.size;
                case message_t.connection_reply:
                    return connection_m_reply.size;
                case message_t.movement:
                    return movement_m.size;
                case message_t.movement_reply:
                    return movement_m_reply.size;
                case message_t.initiate_udp:
                    return initiate_udp_m.size;
                case message_t.initiate_udp_reply:
                    return initiate_udp_m_reply.size;
                case message_t.server_info:
                    return server_info_m.size;
                case message_t.player_load_data:
                    return player_load_data_m.size;
                case message_t.player_disconnect:
                    return player_disconnect_m.size;
                case message_t.racing_lobby_action:
                    return racing_lobby_action_m.size;
                case message_t.racing_lobby_update:
                    return racing_lobby_update_m.size;
                default:
                    return -1;
            }
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

    [StructLayout(LayoutKind.Explicit, Size = size, CharSet = CharSet.Ansi)]
    public unsafe struct generic_m
    {
        public const int size = 256;
        [FieldOffset(0)] private fixed byte bytes[size];
        [FieldOffset(0)] private UInt16 type;
        [FieldOffset(0)] public connection_m connection;
        [FieldOffset(0)] public connection_m_reply connection_reply;
        [FieldOffset(0)] public player_load_data_m player_load_data;
        [FieldOffset(0)] public server_info_m server_info;
        [FieldOffset(0)] public initiate_udp_m initiate_udp;
        [FieldOffset(0)] public initiate_udp_m_reply initiate_udp_reply;
        [FieldOffset(0)] public movement_m movement;
        [FieldOffset(0)] public movement_m_reply movement_reply;
        [FieldOffset(0)] public player_disconnect_m player_disconnect;
        [FieldOffset(0)] public racing_lobby_action_m racing_lobby_action;
        [FieldOffset(0)] public racing_lobby_update_m racing_lobby_update;


        public void from(byte[] data, int n)
        {
            for (int i = 0; i < n; i++)
            {
                if (i >= size || i >= data.Length)
                    break;
                bytes[i] = data[i];
            }
        }
        public void from(byte[] data, int n, int start_index)
        {
            for (int i = 0; i < n; i++)
            {
                int data_offset = i + start_index;
                if (i >= size || data_offset >= data.Length)
                    break;
                bytes[i] = data[data_offset];
            }
        }
        public message_t get_t()
        {
            return (message_t)type;
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = size, CharSet = CharSet.Ansi)]
    public unsafe struct username_sm
    {
        public const int size = 16;
        [FieldOffset(0)] public fixed byte user[15];
        [FieldOffset(15)] public byte flags;
    }

    [StructLayout(LayoutKind.Explicit, Size = size, CharSet = CharSet.Ansi)]
    public unsafe struct position_sm
    {
        public const int size = 6;
        [FieldOffset(0)] public Int16 x;
        [FieldOffset(2)] public Int16 y;
        [FieldOffset(4)] public Int16 z;
    }

    [StructLayout(LayoutKind.Explicit, Size = size, CharSet = CharSet.Ansi)]
    public unsafe struct connection_m
    {
        public const int size = 26;
        [FieldOffset(0)]  public fixed byte bytes[size];

        [FieldOffset(0)]  public UInt16 type;
        [FieldOffset(2)]  public fixed byte key[5];
        [FieldOffset(7)]  public username_sm username;
        [FieldOffset(24)] public UInt16 id;
    }

    [StructLayout(LayoutKind.Explicit, Size = size, CharSet = CharSet.Ansi)]
    public unsafe struct connection_m_reply
    {
        public const int size = 10;
        [FieldOffset(0)] public fixed byte bytes[size];

        [FieldOffset(0)] public UInt16 type;
        [FieldOffset(2)] public position_sm position;
        [FieldOffset(8)] public UInt16 id;
    }

    [StructLayout(LayoutKind.Explicit, Size = size, CharSet = CharSet.Ansi)]
    public unsafe struct player_load_data_m
    {
        public const int size = 26;
        [FieldOffset(0)] public fixed byte bytes[size];

        [FieldOffset(0)] public UInt16 type;
        [FieldOffset(2)] public username_sm username;
        [FieldOffset(18)] public UInt16 id;
        [FieldOffset(20)] public position_sm position;
    }

    [StructLayout(LayoutKind.Explicit, Size = size, CharSet = CharSet.Ansi)]
    public unsafe struct server_info_m
    {
        public const int size = 38;
        [FieldOffset(0)] public fixed byte bytes[size];

        [FieldOffset(0)] public UInt16 type;
        [FieldOffset(2)] public UInt16 server_name;
        [FieldOffset(4)] public fixed byte address[32];
        [FieldOffset(36)] public UInt16 port;
    }

    [StructLayout(LayoutKind.Explicit, Size = size, CharSet = CharSet.Ansi)]
    public unsafe struct movement_m
    {
        public const int size = 18;
        [FieldOffset(0)] public fixed byte bytes[size];

        [FieldOffset(0)] public UInt16 type;
        [FieldOffset(2)] public position_sm position;
        [FieldOffset(8)] public position_sm velocity;
        [FieldOffset(14)] public UInt16 rotation;
        [FieldOffset(16)] public UInt16 timestamp;

    }

    [StructLayout(LayoutKind.Explicit, Size = size, CharSet = CharSet.Ansi)]
    public unsafe struct movement_m_reply
    {
        public const int size = 20;
        [FieldOffset(0)] public fixed byte bytes[size];

        [FieldOffset(0)]  public UInt16 type;
        [FieldOffset(2)]  public UInt16 from_id;
        [FieldOffset(4)]  public position_sm position;
        [FieldOffset(10)] public position_sm velocity;
        [FieldOffset(16)] public UInt16 rotation;
        [FieldOffset(18)] public UInt16 timestamp;
    }

    [StructLayout(LayoutKind.Explicit, Size = size, CharSet = CharSet.Ansi)]
    public unsafe struct initiate_udp_m
    {
        public const int size = 4;
        [FieldOffset(0)] public fixed byte bytes[size];

        [FieldOffset(0)] public UInt16 type;
        [FieldOffset(2)] public UInt16 port;
    }

    [StructLayout(LayoutKind.Explicit, Size = size, CharSet = CharSet.Ansi)]
    public unsafe struct initiate_udp_m_reply
    {
        public const int size = 6;
        [FieldOffset(0)] public fixed byte bytes[size];

        [FieldOffset(0)] public UInt16 type;
        [FieldOffset(2)] public UInt16 port;
        [FieldOffset(4)] public byte granted;
    }

    [StructLayout(LayoutKind.Explicit, Size = size, CharSet = CharSet.Ansi)]
    public unsafe struct player_disconnect_m
    {
        public const int size = 4;
        [FieldOffset(0)] public fixed byte bytes[size];

        [FieldOffset(0)] public UInt16 type;
        [FieldOffset(2)] public UInt16 id;
    }

    [StructLayout(LayoutKind.Explicit, Size = size, CharSet = CharSet.Ansi)]
    public unsafe struct racing_lobby_action_m
    {
        public const int size = 26;
        [FieldOffset(0)] public fixed byte bytes[size];
        [FieldOffset(0)] public UInt16 type;

        [FieldOffset(2)] public UInt16 action; // 0 create lobby, 1 join random, 2 join specific, 3 exit lobby, 4 change username
        [FieldOffset(4)] public fixed byte lobby_code[6];
        [FieldOffset(10)] public username_sm username;
    };

    [StructLayout(LayoutKind.Explicit, Size = size, CharSet = CharSet.Ansi)]
    public unsafe struct racing_lobby_update_m
    {
        public const int size = 30;
        [FieldOffset(0)] public fixed byte bytes[size];
        [FieldOffset(0)] public UInt16 type;

        [FieldOffset(2)] public UInt16 update; // 0 lobby joined, 1 other player joined lobby, 2 other player left lobby, 3 invalid lobby code, 4 ping
        [FieldOffset(4)] public UInt16 other_player_id;
        [FieldOffset(6)] public UInt16 ping;
        [FieldOffset(8)] public fixed byte lobby_code[6];
        [FieldOffset(14)] public username_sm username;
    };
}
