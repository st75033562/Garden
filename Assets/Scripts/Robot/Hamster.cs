namespace Robomation
{
    public static class Hamster
    {
        public const string NAME = "Hamster";
        public const string PRODUCT_ID = "04";
        public const int SENSORY_PACKET_TYPE = 0x00;

    	public const string ID = "kr.robomation.physical.hamster";

        #region devices

        public const int LEFT_WHEEL = 0x00400000;
    	public const int RIGHT_WHEEL = 0x00400001;
    	public const int BUZZER = 0x00400002;
    	public const int OUTPUT_A = 0x00400003;
    	public const int OUTPUT_B = 0x00400004;
    	public const int TOPOLOGY = 0x00400005;
    	public const int LEFT_LED = 0x00400006;
    	public const int RIGHT_LED = 0x00400007;
    	public const int NOTE = 0x00400008;
    	public const int LINE_TRACER_MODE = 0x00400009;
    	public const int LINE_TRACER_SPEED = 0x0040000a;
    	public const int IO_MODE_A = 0x0040000b;
    	public const int IO_MODE_B = 0x0040000c;
    	public const int CONFIG_PROXIMITY = 0x0040000d;
    	public const int CONFIG_GRAVITY = 0x0040000e;
    	public const int CONFIG_BAND_WIDTH = 0x0040000f;
        public const int WHEEL_BALANCE = 0x00400010;

    	public const int SIGNAL_STRENGTH = 0x00400011;
    	public const int LEFT_PROXIMITY = 0x00400012;
    	public const int RIGHT_PROXIMITY = 0x00400013;
    	public const int LEFT_FLOOR = 0x00400014;
    	public const int RIGHT_FLOOR = 0x00400015;
    	public const int ACCELERATION = 0x00400016;
    	public const int LIGHT = 0x00400017;
    	public const int TEMPERATURE = 0x00400018;
    	public const int INPUT_A = 0x00400019;
    	public const int INPUT_B = 0x0040001a;
    	public const int LINE_TRACER_STATE = 0x0040001b;
        public const int BATTERY = 0x0040001c;

        // for gameboard
        public const int LEFT_WHEEL_SPEED = 0x0040001d;
        public const int RIGHT_WHEEL_SPEED = 0x0040001e;

        #endregion devices

        #region acceleration

        public const int ACCEL_X = 0;
        public const int ACCEL_Y = 1;
        public const int ACCEL_Z = 2;

        #endregion acceleration


        #region topology

        public const int TOPOLOGY_NONE = 0;
    	public const int TOPOLOGY_DAISY_CHAIN = 1;
    	public const int TOPOLOGY_STAR = 2;
    	public const int TOPOLOGY_EXTENDED_STAR = 3;

        #endregion topology

        #region led

        public const int LED_OFF = 0;
    	public const int LED_BLUE = 1;
    	public const int LED_GREEN = 2;
    	public const int LED_CYAN = 3;
    	public const int LED_RED = 4;
    	public const int LED_MAGENTA = 5;
    	public const int LED_YELLOW = 6;
    	public const int LED_WHITE = 7;

        #endregion led

        #region line tracer

        public const int LINE_TRACER_MODE_OFF = 0;
    	public const int LINE_TRACER_MODE_BLACK_LEFT_SENSOR = 1;
    	public const int LINE_TRACER_MODE_BLACK_RIGHT_SENSOR = 2;
    	public const int LINE_TRACER_MODE_BLACK_BOTH_SENSORS = 3;
    	public const int LINE_TRACER_MODE_BLACK_TURN_LEFT = 4;
    	public const int LINE_TRACER_MODE_BLACK_TURN_RIGHT = 5;
    	public const int LINE_TRACER_MODE_BLACK_MOVE_FORWARD = 6;
    	public const int LINE_TRACER_MODE_BLACK_UTURN = 7;
    	public const int LINE_TRACER_MODE_WHITE_LEFT_SENSOR = 8;
    	public const int LINE_TRACER_MODE_WHITE_RIGHT_SENSOR = 9;
    	public const int LINE_TRACER_MODE_WHITE_BOTH_SENSORS = 10;
    	public const int LINE_TRACER_MODE_WHITE_TURN_LEFT = 11;
    	public const int LINE_TRACER_MODE_WHITE_TURN_RIGHT = 12;
    	public const int LINE_TRACER_MODE_WHITE_MOVE_FORWARD = 13;
    	public const int LINE_TRACER_MODE_WHITE_UTURN = 14;

        #endregion line tracer

        #region io mode

        public const int IO_MODE_ADC = 0;
    	public const int IO_MODE_DI = 1;
    	public const int IO_MODE_SERVO = 8;
    	public const int IO_MODE_PWM = 9;
    	public const int IO_MODE_DO = 10;

        #endregion io mode

        #region note

        public const int NOTE_OFF = 0;
    	public const int NOTE_A_0 = 1;
    	public const int NOTE_A_SHARP_0 = 2;
    	public const int NOTE_B_FLAT_0 = 2;
    	public const int NOTE_B_0 = 3;
    	public const int NOTE_C_1 = 4;
    	public const int NOTE_C_SHARP_1 = 5;
    	public const int NOTE_D_FLAT_1 = 5;
    	public const int NOTE_D_1 = 6;
    	public const int NOTE_D_SHARP_1 = 7;
    	public const int NOTE_E_FLAT_1 = 7;
    	public const int NOTE_E_1 = 8;
    	public const int NOTE_F_1 = 9;
    	public const int NOTE_F_SHARP_1 = 10;
    	public const int NOTE_G_FLAT_1 = 10;
    	public const int NOTE_G_1 = 11;
    	public const int NOTE_G_SHARP_1 = 12;
    	public const int NOTE_A_FLAT_1 = 12;
    	public const int NOTE_A_1 = 13;
    	public const int NOTE_A_SHARP_1 = 14;
    	public const int NOTE_B_FLAT_1 = 14;
    	public const int NOTE_B_1 = 15;
    	public const int NOTE_C_2 = 16;
    	public const int NOTE_C_SHARP_2 = 17;
    	public const int NOTE_D_FLAT_2 = 17;
    	public const int NOTE_D_2 = 18;
    	public const int NOTE_D_SHARP_2 = 19;
    	public const int NOTE_E_FLAT_2 = 19;
    	public const int NOTE_E_2 = 20;
    	public const int NOTE_F_2 = 21;
    	public const int NOTE_F_SHARP_2 = 22;
    	public const int NOTE_G_FLAT_2 = 22;
    	public const int NOTE_G_2 = 23;
    	public const int NOTE_G_SHARP_2 = 24;
    	public const int NOTE_A_FLAT_2 = 24;
    	public const int NOTE_A_2 = 25;
    	public const int NOTE_A_SHARP_2 = 26;
    	public const int NOTE_B_FLAT_2 = 26;
    	public const int NOTE_B_2 = 27;
    	public const int NOTE_C_3 = 28;
    	public const int NOTE_C_SHARP_3 = 29;
    	public const int NOTE_D_FLAT_3 = 29;
    	public const int NOTE_D_3 = 30;
    	public const int NOTE_D_SHARP_3 = 31;
    	public const int NOTE_E_FLAT_3 = 31;
    	public const int NOTE_E_3 = 32;
    	public const int NOTE_F_3 = 33;
    	public const int NOTE_F_SHARP_3 = 34;
    	public const int NOTE_G_FLAT_3 = 34;
    	public const int NOTE_G_3 = 35;
    	public const int NOTE_G_SHARP_3 = 36;
    	public const int NOTE_A_FLAT_3 = 36;
    	public const int NOTE_A_3 = 37;
    	public const int NOTE_A_SHARP_3 = 38;
    	public const int NOTE_B_FLAT_3 = 38;
    	public const int NOTE_B_3 = 39;
    	public const int NOTE_C_4 = 40;
    	public const int NOTE_C_SHARP_4 = 41;
    	public const int NOTE_D_FLAT_4 = 41;
    	public const int NOTE_D_4 = 42;
    	public const int NOTE_D_SHARP_4 = 43;
    	public const int NOTE_E_FLAT_4 = 43;
    	public const int NOTE_E_4 = 44;
    	public const int NOTE_F_4 = 45;
    	public const int NOTE_F_SHARP_4 = 46;
    	public const int NOTE_G_FLAT_4 = 46;
    	public const int NOTE_G_4 = 47;
    	public const int NOTE_G_SHARP_4 = 48;
    	public const int NOTE_A_FLAT_4 = 48;
    	public const int NOTE_A_4 = 49;
    	public const int NOTE_A_SHARP_4 = 50;
    	public const int NOTE_B_FLAT_4 = 50;
    	public const int NOTE_B_4 = 51;
    	public const int NOTE_C_5 = 52;
    	public const int NOTE_C_SHARP_5 = 53;
    	public const int NOTE_D_FLAT_5 = 53;
    	public const int NOTE_D_5 = 54;
    	public const int NOTE_D_SHARP_5 = 55;
    	public const int NOTE_E_FLAT_5 = 55;
    	public const int NOTE_E_5 = 56;
    	public const int NOTE_F_5 = 57;
    	public const int NOTE_F_SHARP_5 = 58;
    	public const int NOTE_G_FLAT_5 = 58;
    	public const int NOTE_G_5 = 59;
    	public const int NOTE_G_SHARP_5 = 60;
    	public const int NOTE_A_FLAT_5 = 60;
    	public const int NOTE_A_5 = 61;
    	public const int NOTE_A_SHARP_5 = 62;
    	public const int NOTE_B_FLAT_5 = 62;
    	public const int NOTE_B_5 = 63;
    	public const int NOTE_C_6 = 64;
    	public const int NOTE_C_SHARP_6 = 65;
    	public const int NOTE_D_FLAT_6 = 65;
    	public const int NOTE_D_6 = 66;
    	public const int NOTE_D_SHARP_6 = 67;
    	public const int NOTE_E_FLAT_6 = 67;
    	public const int NOTE_E_6 = 68;
    	public const int NOTE_F_6 = 69;
    	public const int NOTE_F_SHARP_6 = 70;
    	public const int NOTE_G_FLAT_6 = 70;
    	public const int NOTE_G_6 = 71;
    	public const int NOTE_G_SHARP_6 = 72;
    	public const int NOTE_A_FLAT_6 = 72;
    	public const int NOTE_A_6 = 73;
    	public const int NOTE_A_SHARP_6 = 74;
    	public const int NOTE_B_FLAT_6 = 74;
    	public const int NOTE_B_6 = 75;
    	public const int NOTE_C_7 = 76;
    	public const int NOTE_C_SHARP_7 = 77;
    	public const int NOTE_D_FLAT_7 = 77;
    	public const int NOTE_D_7 = 78;
    	public const int NOTE_D_SHARP_7 = 79;
    	public const int NOTE_E_FLAT_7 = 79;
    	public const int NOTE_E_7 = 80;
    	public const int NOTE_F_7 = 81;
    	public const int NOTE_F_SHARP_7 = 82;
    	public const int NOTE_G_FLAT_7 = 82;
    	public const int NOTE_G_7 = 83;
    	public const int NOTE_G_SHARP_7 = 84;
    	public const int NOTE_A_FLAT_7 = 84;
    	public const int NOTE_A_7 = 85;
    	public const int NOTE_A_SHARP_7 = 86;
    	public const int NOTE_B_FLAT_7 = 86;
    	public const int NOTE_B_7 = 87;
    	public const int NOTE_C_8 = 88;

        #endregion
    }
}
