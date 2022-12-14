using PK3_RAM_Injection.Model;

namespace PK3_RAM_Injection.Controller
{
    class Validation_Data_Check
    {
        readonly Data_Conversion hex;
        readonly Offest_data offset_Data;
        readonly Bit_Check bit;

        public Validation_Data_Check()
        {
            hex = new();
            offset_Data = new();
            bit = new();
        }

        /// <summary>
        /// Runs actual checksum if there is one else return true
        /// because that game was a big pain (cough cough Colo and XD)
        /// and the developers didn't feel the need for checksums
        /// therefore leaving them up so I had to make stuff up!!!!!
        /// </summary>
        /// <param name="inputFile">The data being checked</param>
        /// <param name="option">Only matters if it is 0</param>
        /// <param name="size">The size of the data</param>
        /// <param name="inversion">little edian?</param>
        /// <returns></returns>
        public bool ChecksumEnd(byte[] inputFile, int option, int size, bool inversion)
        {
            if (option == 0) //Calcs checksum to see if it matches the value stored in data
            {
                if (Checksum(inputFile, offset_Data.Checksum, offset_Data.ChecksumCalcDataStart, size, inversion))
                    return true;
            }
            else
                return true; //Return true if option is not 0 because there is no checksum

            return false;
        }

        /// <summary>
        /// A frankenstine function of stuff from PKHeX
        /// and my own insanity
        /// </summary>
        /// <param name="data"></param>
        /// <param name="chkOffest"></param>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="inversion"></param>
        /// <returns></returns>
        public bool Checksum(byte[] data, int chkOffest, int start, int size, bool inversion)
        {
            Data_Conversion con = new Data_Conversion();
            ushort check = PokeCrypto.GetCHK(data, start, size); //Makes the checksum

            if (con.LittleEndian(data, chkOffest, 2, inversion) == check)
                return true;

            return false;
        }

        /// <summary>
        /// Ensures the IVs are valid
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public bool IVs(byte[] inputFile, int option)
        {
            if (option == 0) //IVs stored in 3 bytes
            {
                if (bit.IV(inputFile, offset_Data.IV))
                    return true;
            }
            else if (option == 1) //IVs stored in 6 bytes
            {
                if (inputFile[offset_Data.IV] <= 31 &&
                    inputFile[offset_Data.IV + offset_Data.SizeIV] <= 31 &&
                    inputFile[offset_Data.IV + offset_Data.SizeIV * 2] <= 31 &&
                    inputFile[offset_Data.IV + offset_Data.SizeIV * 3] <= 31 &&
                    inputFile[offset_Data.IV + offset_Data.SizeIV * 4] <= 31 &&
                    inputFile[offset_Data.IV + offset_Data.SizeIV * 5] <= 31)
                    return true;
            }
            else if (option == 2 || option == 3)
            {
                return true; //Checking IVs in gens 1 and 2 is pointless
            }

            return false;
        }

        /// <summary>
        /// Checks if Pkrus is valid
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public bool Pkrus(byte[] inputFile, int option)
        {
            if (option == 0 || option == 3) //PkRus single byte
            {
                return Bit_Check.Pokerus(inputFile[offset_Data.Pkrus]);
            }
            else if (option == 1) //PkRus Split in 2 bytes
            {
                int skip;
                if (offset_Data.Pkrus == 0xCA)
                    skip = 3; //Colo
                else
                    skip = 2; //XD

                byte[] tempArr = { inputFile[offset_Data.Pkrus] };
                string byte1 = Convert.ToHexString(tempArr);
                tempArr[0] = inputFile[offset_Data.Pkrus + skip];
                string byte2 = Convert.ToHexString(tempArr);
                byte pkrus;

                byte1 += byte2;
                tempArr = Convert.FromHexString(byte1);
                pkrus = tempArr[0];
                return Bit_Check.Pokerus(pkrus);
            }
            else if (option == 2)
                return true;

            return false;
        }

        /// <summary>
        /// Ensures the EV total is valid
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="totalEV"></param>
        /// <param name="invert"></param>
        /// <param name="gen"></param>
        /// <param name="subGen"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public bool EV(byte[] buffer, int totalEV, bool invert, int gen, int subGen, int option)
        {
            offset_Data.SetValues(gen, subGen);

            if (hex.LittleEndian(buffer, offset_Data.HPEV, offset_Data.SizeHPEV, invert) +
                hex.LittleEndian(buffer, offset_Data.AttEV, offset_Data.SizeAttEV, invert) +
                hex.LittleEndian(buffer, offset_Data.DefEV, offset_Data.SizeDefEV, invert) +
                hex.LittleEndian(buffer, offset_Data.SpeedEV, offset_Data.SizeSpeedEV, invert) +
                hex.LittleEndian(buffer, offset_Data.SpAttEV, offset_Data.SizeSpAttEV, invert) +
                hex.LittleEndian(buffer, offset_Data.SpDefEV, offset_Data.SizeSpDefEV, invert) <= totalEV)
                return true;

            return false;
        }
    }
}
