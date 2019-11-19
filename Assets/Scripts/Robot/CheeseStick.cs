namespace Robomation
{
    public static class CheeseStick
    {
        public const string NAME = "Cheese Stick";
        public const string PRODUCT_ID = "0D";
        public const int SENSORY_PACKET_TYPE = 0x10;

        public const string ID = "kr.robomation.physical.cheesestick";

        #region output devices

        public const int CONFIG_SA = 0;
        public const int CONFIG_SB = 1;
        public const int CONFIG_SC = 2;

        public const int CONFIG_L_MODE = 3;
        public const int CONFIG_LA     = 4;
        public const int CONFIG_LB     = 5;
        public const int CONFIG_LC     = 6;
        public const int DEVICE_ID     = 7;

        public const int CONFIG_M_MODE  = 8;
        public const int CONFIG_M_STEP  = 9;
        public const int CONFIG_M_CYCLE = 10;

        public const int G_RANGE   = 11;
        public const int BANDWIDTH = 12;

        public const int OUT_SA = 13;
        public const int OUT_SB = 14;
        public const int OUT_SC = 15;

        // valid when Sabc set to digital or analog
        public const int PULL_SA = 16;
        public const int PULL_SB = 17;
        public const int PULL_SC = 18;

        public const int ADC_SA = 19;
        public const int ADC_SB = 20;
        public const int ADC_SC = 21;

        public const int OUT_LA = 22;
        public const int OUT_LB = 23;
        public const int OUT_LC = 24;

        public const int OUT_MA = 25;
        public const int OUT_MB = 26;
        public const int PPS    = 27;
        public const int PULSES = 28;

        public const int CLEAR_ENCODER = 29;
        public const int CLEAR_STEP    = 30;

        public const int BUZZ       = 31;
        public const int PIANO_NOTE = 32;
        public const int SOUND_CLIP = 33;
        public const int SOUND_OUT  = 34;

        #endregion output devices

        #region input devices

        public const int INPUT_A         = 101;
        public const int INPUT_B         = 102;
        public const int INPUT_C         = 103;
        public const int INPUT_LA        = 104;
        public const int INPUT_LB        = 105;
        public const int INPUT_LC        = 106;
        public const int ECHO            = 107;
        public const int ACCELERATION    = 108;
        public const int STEP_COUNTER    = 109;
        public const int FREE_FALL_ID    = 110;
        public const int TAP_ID          = 111;
        public const int TEMPERATURE     = 112;
        public const int POWER_STATE     = 113;
        public const int PLAY_STATE      = 114;
        public const int STEP_STATE      = 115;
        public const int SIGNAL_STRENGTH = 116;
        public const int BATTERY_LEVEL   = 117;

        #endregion input devices

        public const int L_MODE_NORMAL      = 0;
        public const int L_MODE_ULTRA_SONIC = 1;
        public const int L_MODE_ENCODER     = 2;
        public const int L_MODE_UART_9600   = 3;

    	public const int S_MODE_DIGITAL = 0;
        public const int S_MODE_ANALOG  = 1;
    	public const int S_MODE_PWM     = 2;
    	public const int S_MODE_SERVO   = 3;

        public const int PULL_DOWN = 0;
        public const int PULL_UP   = 1;

        public const int ADC_VOLTAGE_POWER_REF = 0;
        public const int ADC_VOLTAGE_REF       = 1;

        #region config m

        public const int M_MODE_DC_MOTOR   = 0;
        public const int M_MODE_MONO_SERVO = 1;
        public const int M_MODE_DUAL_SERVO = 2;
        public const int M_MODE_STEP_MOTOR = 3;

        public const int CYCLE_50HZ  = 0;
        public const int CYCLE_100HZ = 1;
        public const int CYCLE_200HZ = 2;
        public const int CYCLE_400HZ = 3;

        public const int STEP_MODE_OFF        = 0;
        public const int STEP_MODE_WAVE_DRIVE = 1;
        public const int STEP_MODE_FULL_STEP  = 2;
        public const int STEP_MODE_HALF_STEP  = 3;

        #endregion config m

        #region device ids

        public const int DEVICE_UART_9600      = 0x40;
        public const int DEVICE_ULTRASONIC     = 0x81;
        public const int DEVICE_ENCODER        = 0x82;
        public const int DEVICE_NEO_PIXEL_RGB  = 0xC1;
        public const int DEVICE_NEO_PIXEL_RGBW = 0xC2;

        #endregion device ids

        #region bandwidth

        public const int BANDWIDTH_7_81HZ  = 0;
        public const int BANDWIDTH_15_36HZ = 1;
        public const int BANDWIDTH_31_25HZ = 2;
        public const int BANDWIDTH_62_5HZ  = 3;
        public const int BANDWIDTH_125HZ   = 4;
        public const int BANDWIDTH_250HZ   = 5;
        public const int BANDWIDTH_500HZ   = 6;
        public const int BANDWIDTH_1000HZ  = 7;

        #endregion bandwidth

        #region g range

        public const int GRANGE_2G  = 0;
        public const int GRANGE_4G  = 1;
        public const int GRANGE_8G  = 2;
        public const int GRANGE_16G = 3;

        #endregion g range


        #region sound out

        public const int SOUND_OUT_PIEZO = 0;
        public const int SOUND_OUT_SA    = 1;
        public const int SOUND_OUT_SB    = 2;
        public const int SOUND_OUT_SC    = 3;

        #endregion

        #region acceleration

        public const int ACCEL_X = 0;
        public const int ACCEL_Y = 1;
        public const int ACCEL_Z = 2;

        #endregion acceleration

        #region clip number

        public const int BUZZ_MUTE          = 0;
        public const int BUZZ_BEEP          = 1;
        public const int BUZZ_BEEP2         = 2;
        public const int BUZZ_BEEP3         = 3;
        public const int BUZZ_BEEP_REP      = 4;
        public const int BUZZ_BEEP_RND      = 0x05;
        public const int BUZZ_BEEP_RND_REP  = 0x06;
        public const int BUZZ_SNORE         = 0x07;
        public const int BUZZ_SNORE_REP     = 0x08;
        public const int BUZZ_SIREN         = 0x09;
        public const int BUZZ_SIREN_REP     = 0x0A;
        public const int BUZZ_ENGINE        = 0x0B;
        public const int BUZZ_ENGINE_REP    = 0x0C;
        public const int BUZZ_FART_A        = 0x0D;
        public const int BUZZ_FART_B        = 0x0E;
        public const int BUZZ_NOISE         = 0x0F;
        public const int BUZZ_NOISE_REP     = 0x10;
        public const int BUZZ_WISTLE        = 0x11;
        public const int BUZZ_CHOP_CHOP     = 0x12;
        public const int BUZZ_CHOP_CHOP_REP = 0x13;

        public const int CLIP_R2D2          = 0x20;
        public const int CLIP_DIBIDIBIDIP   = 0x21;
        public const int CLIP_SIMPLE_MELODY = 0x22;
        public const int CLIP_FINISH        = 0x23;

        public const int MELODY_HAPPY_MOOD    = 0x30;
        public const int MELODY_ANGRY_MOOD    = 0x31;
        public const int MELODY_SAD_MOOD      = 0x32;
        public const int MELODY_SLEEP_MOOD    = 0x33;
        public const int MELODY_TOY_MARCH     = 0x34;
        public const int MELODY_BIRTHDAY_SONG = 0x35;

        #endregion clip number

        #region internal state

        public const int POWER_NORMAL   = 0;
        public const int POWER_LOW      = 1;
        public const int POWER_DISCHARE = 2;

        public const int SOUND_STOPPED = 0;
        public const int SOUND_PLAYING = 1;

        public const int STEP_MOTOR_OFF = 0;
        public const int STEP_MOTOR_ON  = 1;

        #endregion interal state
    }
}
