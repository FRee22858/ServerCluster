﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataProperty
{
    public enum ValueType
    {
        INT,
        FLOAT,
        STRING
    }
    public class Property:ICloneable
    {
        private ValueType _type;

        public ValueType Type
        {
            get { return _type; }
            set { _type = value; }
        }
        private string _key;

        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }
        private object _value;

        public object Value
        {
            get { return _value; }
        }

        public Property(ValueType type,string key,object value)
        {
            Set(type, key, value);
        }

        private void Set(ValueType type, string key, object value)
        {
            _type = type;
            _key = key;
            _value = value;
        }

        public object Clone()
        {
            var clone = new Property(_type, _key, _value);
            //clone._type = _type;
            //clone._key = _key;
            //clone._value = _value;

            return clone;
        }
    }
}
