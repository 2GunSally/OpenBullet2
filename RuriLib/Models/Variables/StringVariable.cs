﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RuriLib.Models.Variables;

public class StringVariable : Variable
{
    private readonly string value;

    public StringVariable(string value)
    {
        this.value = value;
        Type = VariableType.String;
    }

    public override string AsString() => value;

    public override int AsInt()
    {
        if (int.TryParse(value, out var result))
            return result;
        throw new InvalidCastException();
    }

    public override bool AsBool()
    {
        if (bool.TryParse(value, out var result))
            return result;
        throw new InvalidCastException();
    }

    public override byte[] AsByteArray() => Encoding.UTF8.GetBytes(value);

    public override float AsFloat()
    {
        if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
        throw new InvalidCastException();
    }

    public override List<string> AsListOfStrings() => new() { value };

    public override object AsObject() => value;
}