﻿using System;

namespace KidsTown.Database.EfCore;

public class TaskExecution
{
    public int Id { get; set; }

    public DateTime InsertDate { get; set; }

    public bool IsSuccess { get; set; }

    public int UpdateCount { get; set; }

    public string Environment { get; set; } = null!;

    public string TaskName { get; set; } = null!;
}