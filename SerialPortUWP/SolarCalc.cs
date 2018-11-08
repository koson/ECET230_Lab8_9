using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortUWP
{
    class SolarCalc {

        //Field
        private static int resistorValue;
        private static double vRef;

        private NumberFormatInfo provider = new NumberFormatInfo();


        //Constructor
        public SolarCalc() {
            resistorValue = 100;    //R1
            vRef = 4.78;            //Measure my own vRef
        }



        //Methods
        public string GetSolarVoltage(int an0) {
            double dAn0 = an0 * vRef / 1024.0;
            return dAn0.ToString("0.0000");
        }

        public string GetBatteryVoltage(int an2) {
            double dAn2 = an2 * vRef / 1024.0;
            return dAn2.ToString("0.0000");
        }

        public string GetBatteryCurrent(int an1, int an2) {
            int shuntAnalog = an1 - an2;
            double shuntVoltage = shuntAnalog * vRef / 1024.0;
            double dBatCurrent = shuntVoltage / resistorValue;
            return dBatCurrent.ToString("0.000000");
        }

        public string GetLedCurrent(int ledAnalog, int an1) {
            int shuntAnalog = an1 - ledAnalog;
            double shuntVoltage = shuntAnalog * vRef / 1024.0;
            double dLedCurrent = shuntVoltage / resistorValue;
            if (dLedCurrent < 0.0001) dLedCurrent = 0;
            return dLedCurrent.ToString("0.000000");
        }
    }
}
