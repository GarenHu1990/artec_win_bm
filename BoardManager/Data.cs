﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace ScratchConnection
{
    enum ConnectingCondition : byte
    {
        CONNECTED,      // 接続中
        DISCONNECT,     // 切断
    }

    // ---------------------------------------------------------------------
    // ロボット入力センサーのID
    // ---------------------------------------------------------------------
    enum OptionPartsID : int
    {
        Top = -1,
        // センサーIDの設定
        SensorTop = 0,  // センサーIDの先頭
        Light = SensorTop,      //  0: 光
        Touch,                  //  1: タッチ
        Sound,                  //  2; 音
        IRReflect,              //  3: 赤外線近距離
        Accelerometer,         //  4: 加速度(X軸)
        SensorEnd = Accelerometer,
        // LED IDの設定
        LEDTop = SensorEnd + 1, // LEDIDの先頭
        LED = LEDTop,           //  9: LED
        LEDEnd = LEDTop,
        // ブザーIDの設定
        BuzzerTop = LEDEnd + 1, // ブザーIDの先頭
        Buzzer = BuzzerTop,     // 10: ブザー
        BuzzerEnd = Buzzer,
        // カラーLED IDの設定
        ColorLEDTop = BuzzerEnd + 1, // カラーLEDIDの先頭
        ColorLED = ColorLEDTop,           //  9: LED
        ColorLEDEnd = ColorLEDTop,
        // ボタンIDの設定
        ButtonTop = ColorLEDEnd + 1, // ボタンIDの先頭
        Button1 = ButtonTop,    // 11: ボタン1
        Button2,                // 12: ボタン2
        Button3,                // 13: ボタン3
        Button4,                // 14: ボタン4
        ButtonEnd = Button4,

        End = ButtonEnd
    }

    // ---------------------------------------------------------------------
    // スクラッチ仕様のパーツID
    // ---------------------------------------------------------------------
    enum ScartchPartsID : int
    {
        Open = 0x00,            // 開放
        LED = 0x03,             // LED
        Buzzer = 0x04,          // ブザー
        ColorLED = 0x05,        // カラーLED
        Light = 0x10,           // 光
        Touch = 0x11,           // タッチ
        Sound = 0x12,           // 音
        IRReflect = 0x13,       // 赤外線近距離
        Accelerometer = 0x14,   // 加速度(X軸)
        Button = 0x15             // ボタン
    }

    // ---------------------------------------------------------------------
    // 多言語対応
    // ---------------------------------------------------------------------
    class SensorItems
    {
        // strOptionPartsは、上にあるOptionPartsIDと整合性が取れる事
        public string[] strOptionParts = new string[(int)(OptionPartsID.End + 1)] {
            Properties.Resources.str_parts_sensor_light,   //  光センサー
            Properties.Resources.str_parts_sensor_touch,   //  タッチセンサー
            Properties.Resources.str_parts_sensor_sound,   //  音センサー
            Properties.Resources.str_parts_sensor_ir,      //  赤外線反射センサー
            Properties.Resources.str_parts_sensor_acc,     //  加速度センサー
            Properties.Resources.str_parts_dev_led,        //  LED
            Properties.Resources.str_parts_dev_buzzer,     //  ブザー
            Properties.Resources.str_parts_dev_colorLed,   //  カラーLED
            Properties.Resources.str_parts_button1,        //  ボタン1
            Properties.Resources.str_parts_button2,        //  ボタン2
            Properties.Resources.str_parts_button3,        //  ボタン3
            Properties.Resources.str_parts_button4         //  ボタン4
        };
        // ひらがな表示用
        public string[] strOptionPartsHiragana = new string[(int)(OptionPartsID.End + 1)] {
            "ひかりセンサー",
            "タッチセンサー",
            "おとセンサー",
            "せきがいせんフォトリフレクタ",
            "かそくどセンサー",
            "LED",
            "ブザー",
            "カラーLED",    // 未使用
            "ボタン1",
            "ボタン2",
            "ボタン3",
            "ボタン4"
        };
    }

    // ---------------------------------------------------------------------
    // ロボット入出力管理
    // ---------------------------------------------------------------------
    public class stRobotIOStatus
    {
        SensorItems si = new SensorItems();

        // DCモータの設定状態
        public bool fDCMotor1Used;
        public bool fDCMotor2Used;
        // サーボモータの設定状態
        public bool fSvMotor1Used;
        public bool fSvMotor2Used;
        public bool fSvMotor3Used;
        public bool fSvMotor4Used;
        public bool fSvMotor5Used;
        public bool fSvMotor6Used;
        public bool fSvMotor7Used;
        public bool fSvMotor8Used;
        // センサーの設定状態
        public bool fSns1Used;
        public bool fSns2Used;
        public bool fSns3Used;
        public bool fSns4Used;
        public bool fSns5Used;
        public bool fSns6Used;
        public bool fSns7Used;
        public bool fSns8Used;
        // センサーの種類
        public int nSns1Kind;
        public int nSns2Kind;
        public int nSns3Kind;
        public int nSns4Kind;
        public int nSns5Kind;
        public int nSns6Kind;
        public int nSns7Kind;
        public int nSns8Kind;
        // ボタンの設定状態
        public bool fBtn1Used;
        public bool fBtn2Used;
        public bool fBtn3Used;
        public bool fBtn4Used;

        bool[] userPinStatus = new bool[8];         // ユーザピン(センサー用コネクタ)の状態  true:接続、false:未接続
        int[] userPinConnectedParts = new int[8];   // ユーザピンに接続されている機器

        // ---------------------------------------------------------------------
        // 概要	        ：入出力ポートを初期化する
        // 備考         ：
        // Date         ：2013/7/8	0.01	kagayama  新規作成
        // ---------------------------------------------------------------------
        public void initRobotIOConfiguration()
        {
            fDCMotor1Used = true;
            fDCMotor2Used = true;
            fSvMotor1Used = true;
            fSvMotor2Used = true;
            fSvMotor3Used = true;
            fSvMotor4Used = true;
            fSvMotor5Used = false;
            fSvMotor6Used = false;
            fSvMotor7Used = false;
            fSvMotor8Used = false;
            fSns1Used = false;
            fSns2Used = false;
            fSns3Used = false;
            fSns4Used = false;
            fSns5Used = true;
            fSns6Used = true;
            fSns7Used = true;
            fSns8Used = true;
            nSns1Kind = (int)OptionPartsID.Light;
            nSns2Kind = (int)OptionPartsID.Light;
            nSns3Kind = (int)OptionPartsID.Light;
            nSns4Kind = (int)OptionPartsID.Light;
            nSns5Kind = (int)OptionPartsID.LED;
            nSns6Kind = (int)OptionPartsID.Buzzer;
            nSns7Kind = (int)OptionPartsID.Light;
            nSns8Kind = (int)OptionPartsID.Light;
            fBtn1Used = true;
            fBtn2Used = true;
            fBtn3Used = true;
            fBtn4Used = true;
        }

        // ---------------------------------------------------------------------
        // 概要	        ：入出力状態をスクラッチ用のフォーマットで返す
        // 備考         ：
        // Date         ：2013/8/24	0.01	kagayama  新規作成
        // ---------------------------------------------------------------------
        public int convertIOConfPartsID(int scratchPartsID)
        {
            int dat = -1;
            switch (scratchPartsID)
            {
                case (int)ScartchPartsID.LED:
                    dat = (int)OptionPartsID.LED;
                    break;
                case (int)ScartchPartsID.Buzzer:
                    dat = (int)OptionPartsID.Buzzer;
                    break;
                case (int)ScartchPartsID.ColorLED:
                    dat = (int)OptionPartsID.ColorLED;
                    break;
                case (int)ScartchPartsID.Light:
                    dat = (int)OptionPartsID.Light;
                    break;
                case (int)ScartchPartsID.Touch:
                    dat = (int)OptionPartsID.Touch;
                    break;
                case (int)ScartchPartsID.Sound:
                    dat = (int)OptionPartsID.Sound;
                    break;
                case (int)ScartchPartsID.IRReflect:
                    dat = (int)OptionPartsID.IRReflect;
                    break;
                case (int)ScartchPartsID.Accelerometer:
                    dat = (int)OptionPartsID.Accelerometer;
                    break;
                default:
                    break;
            }
            return dat;
        }

        private void setButtonState(int i, bool f)
        {
            if (i == 0) fBtn1Used = f;
            if (i == 1) fBtn2Used = f;
            if (i == 2) fBtn3Used = f;
            if (i == 3) fBtn4Used = f;
        }
        private void setSensorState(int i, bool f)
        {
            if (i == 0) fSns1Used = f;
            if (i == 1) fSns2Used = f;
            if (i == 2) fSns3Used = f;
            if (i == 3) fSns4Used = f;
            if (i == 4) fSns5Used = f;
            if (i == 5) fSns6Used = f;
            if (i == 6) fSns7Used = f;
            if (i == 7) fSns8Used = f;
        }
        private void setSensorKind(int i, int v)
        {
            if (i == 0) nSns1Kind = v;
            if (i == 1) nSns2Kind = v;
            if (i == 2) nSns3Kind = v;
            if (i == 3) nSns4Kind = v;
            if (i == 4) nSns5Kind = v;
            if (i == 5) nSns6Kind = v;
            if (i == 6) nSns7Kind = v;
            if (i == 7) nSns8Kind = v;
        }

        // ---------------------------------------------------------------------
        // 概要	        ：入出力状態をスクラッチ用のフォーマットで返す
        // 備考         ：
        // Date         ：2013/8/24	0.01	kagayama  新規作成
        // ---------------------------------------------------------------------
        public void setStatusByte(byte[] scratchData)
        {
            // DCモータの設定 [0, 1]
            fDCMotor1Used = ((scratchData[0] & 0xFF) != 0);    // 下位8ビットが開放(0)でなければ、DCモータが接続されている
            fDCMotor2Used = ((scratchData[1] & 0xFF) != 0);
            // サーボモータの設定 [2～9]
            fSvMotor5Used = ((scratchData[2] & 0xFF) != 0);    // 下位8ビットが開放(0)でなければ、サーボモータが接続されている
            fSvMotor6Used = ((scratchData[3] & 0xFF) != 0);
            fSvMotor7Used = ((scratchData[4] & 0xFF) != 0);
            fSvMotor8Used = ((scratchData[5] & 0xFF) != 0);
            fSvMotor1Used = ((scratchData[6] & 0xFF) != 0);
            fSvMotor2Used = ((scratchData[7] & 0xFF) != 0);
            fSvMotor3Used = ((scratchData[8] & 0xFF) != 0);
            fSvMotor4Used = ((scratchData[9] & 0xFF) != 0);
            // センサーピンA0～A7の設定 [10～17]
            for (int i = 0;i < 8;i++) {
                byte partsID = (byte)(scratchData[10 + i] & 0xFF);
                switch (partsID) {
                    case (byte)ScartchPartsID.Open:  // 開放
                        if (i < 4) { setButtonState(i, false); }
                        setSensorState(i, false);
                        setSensorKind(i, (int)OptionPartsID.Light);   // デフォルト値
                        break;
                    case (byte)ScartchPartsID.Button:  // ボタンが設定
                        if (i < 4) { setButtonState(i, true); }
                        setSensorState(i, false);
                        setSensorKind(i, (int)OptionPartsID.Light);   // デフォルト値
                        break;
                    default:    // その他
                        if (i < 4) { setButtonState(i, false); }
                        setSensorState(i, true);
                        setSensorKind(i, convertIOConfPartsID(partsID));
                        break;
                }
            }
        }

        // ---------------------------------------------------------------------
        // 概要	        ：入出力状態をスクラッチ用のフォーマットで返す
        // 備考         ：
        // Date         ：2013/8/24	0.01	kagayama  新規作成
        // ---------------------------------------------------------------------
        public List<byte> getStatusByte()
        {
            List<byte> stat = new List<byte>();
            bool[] pinsDCSV = {
                              fDCMotor1Used, fDCMotor2Used,
                              fSvMotor5Used, fSvMotor6Used, fSvMotor7Used, fSvMotor8Used,
                              fSvMotor1Used, fSvMotor2Used, fSvMotor3Used, fSvMotor4Used
                          };

            bool[] pinsBT = { fBtn1Used, fBtn2Used, fBtn3Used, fBtn4Used };
            bool[] pinsSNS = { fSns1Used, fSns2Used, fSns3Used, fSns4Used, fSns5Used, fSns6Used, fSns7Used, fSns8Used };
            int[] kinds = { nSns1Kind, nSns2Kind, nSns3Kind, nSns4Kind, nSns5Kind, nSns6Kind, nSns7Kind, nSns8Kind };

            byte dat;

            // モーターの処理
            for (int i = 0; i < 10; i++)
            {
                dat = (byte)0x00;
                // DCモータ
                if (i < 2)
                    dat = pinsDCSV[i] ? (byte)0x01 : (byte)0x00;
                // サーボモータ
                else
                    dat = pinsDCSV[i] ? (byte)0x02 : (byte)0x00;
                stat.Add(dat);
            }
            // センサ・デバイスの処理
            for (int i = 0; i < 8; i++)
            {
                dat = (byte)0x00;
                // ボタン
                if (i < 4 && pinsBT[i])
                {
                    dat = (byte)0x15;
                }
                // センサー
                else
                {
                    if (pinsSNS[i])
                    {
                        switch (kinds[i])
                        {
                            case (int)OptionPartsID.LED:
                                dat = (byte)0x03;
                                break;
                            case (int)OptionPartsID.Buzzer:
                                dat = (byte)0x04;
                                break;
                            case (int)OptionPartsID.ColorLED:
                                dat = (byte)0x05;
                                break;
                            case (int)OptionPartsID.Light:
                                dat = (byte)0x10;
                                break;
                            case (int)OptionPartsID.Touch:
                                dat = (byte)0x11;
                                break;
                            case (int)OptionPartsID.Sound:
                                dat = (byte)0x12;
                                break;
                            case (int)OptionPartsID.IRReflect:
                                dat = (byte)0x13;
                                break;
                            case (int)OptionPartsID.Accelerometer:
                                dat = (byte)0x14;
                                break;
                            default:
                                break;
                        }

                    }
                }
                stat.Add(dat);
            }
            return stat;
        }
    }

    // ---------------------------------------------------------------------
    // 概要	        ：サーボモーターオフセット管理クラス
    // 備考         ：
    // Date         ：2014/01/29	0.94	kagayama  新規作成
    // ---------------------------------------------------------------------
    public class ServoOffset
    {
        int[] offset = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        DCCalibInfo dcInfo;

        public void reset()
        {
            for (int i = 0; i < offset.Length; i++)
            {
                offset[i] = 0;
            }
        }

        public void set(int[] offset)
        {
            for (int i = 0; i < offset.Length; i++)
            {
                this.offset[i] = offset[i];
            }
        }

        public void set(int index, int value)
        {
            offset[index] = value;
        }

        public void setDCCalib(byte m1Rate, byte m2Rate)
        {
            dcInfo.calibM1Rate = m1Rate;
            dcInfo.calibM2Rate = m2Rate;
        }


        public int getValue(int index)
        {
            return offset[index];
        }

        public int[] getValues()
        {
            return offset;
        }
   
        public DCCalibInfo getDCCalibInfo()
        {
            return dcInfo;
        }
    }
    // ---------------------------------------------------------------------
    // DCモーター校正情報
    // ---------------------------------------------------------------------
    public struct DCCalibInfo
    {
        public byte calibM1Rate;    // 50%(127)～100%(255)の整数
        public byte calibM2Rate;    // 50%(127)～100%(255)の整数
    }
}