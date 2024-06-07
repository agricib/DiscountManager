namespace DiscountManager
{
    public class DiscountCodeManager
    {
        private readonly string _storageFile;
        public readonly List<DiscountCode> _codes;

        public DiscountCodeManager(string storageFile)
        {
            _storageFile = storageFile;
            _codes = LoadCodes();
        }

        public List<DiscountCode> GenerateCodes(ushort count, byte length)
        {
            if (count > 2000)
                throw new ArgumentException("Maximum 2000 codes allowed");

            var generated = 0;
            while (generated < count)
            {
                var code = GenerateRandomCode(length);
                if (!_codes.Any(c => c.Code == code))
                {
                    _codes.Add(new DiscountCode { Code = code, Used = false });
                    generated++;
                }
            }

            SaveCodes();
            return _codes.GetRange(generated - count, count); // Return only generated codes
        }

        public bool UseCode(string code)
        {
            var discount = _codes.FirstOrDefault(c => c.Code == code);
            if (discount == null)
                return false;

            if (discount.Used)
                return false;

            discount.Used = true;
            SaveCodes();
            return true;
        }

        private string GenerateRandomCode(byte length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var code = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return code;
        }

        public List<DiscountCode> LoadCodes()
        {
            if (!File.Exists(_storageFile))
                return new List<DiscountCode>();

            var lines = File.ReadAllLines(_storageFile);
            return lines.Select(line =>
            {
                var parts = line.Split(',');
                return new DiscountCode { Code = parts[0], Used = bool.Parse(parts[1]) };
            }).ToList();
        }

        public void SaveCodes()
        {
            var lines = _codes.Select(code => code.Code + "," + code.Used);
            File.WriteAllLines(_storageFile, lines);
        }
    }
}
