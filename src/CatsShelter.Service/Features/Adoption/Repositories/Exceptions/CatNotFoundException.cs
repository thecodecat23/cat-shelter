﻿namespace CatsShelter.Service.Features.Adoption.Repositories.Exceptions;

public class CatNotFoundException : Exception
{
    public CatNotFoundException(string id) : base($"No cat with id {id} was found.")
    {
    }
}