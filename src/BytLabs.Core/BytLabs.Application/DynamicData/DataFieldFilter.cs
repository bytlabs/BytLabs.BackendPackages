﻿namespace BytLabs.Application.DynamicData
{
    public class DataFieldFilter
    {
        public string Path { get; set; } = string.Empty;
        public FilterOperation Operation { get; set; }
        public ValueKind ValueType { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
