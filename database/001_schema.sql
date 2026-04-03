-- Task Management System - SQLite Schema
-- Tables: Users, Tasks

CREATE TABLE IF NOT EXISTS Users (
    Id TEXT PRIMARY KEY NOT NULL,
    Username TEXT NOT NULL UNIQUE,
    Email TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS Tasks (
    Id TEXT PRIMARY KEY NOT NULL,
    Title TEXT NOT NULL,
    Description TEXT,
    Status INTEGER NOT NULL DEFAULT 0,  -- 0=Todo, 1=InProgress, 2=Done
    DueDate TEXT NOT NULL,
    AssignedUserId TEXT,
    CreatedByUserId TEXT NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    FOREIGN KEY (AssignedUserId) REFERENCES Users(Id),
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
);

CREATE INDEX IF NOT EXISTS IX_Tasks_AssignedUserId ON Tasks(AssignedUserId);
CREATE INDEX IF NOT EXISTS IX_Tasks_CreatedByUserId ON Tasks(CreatedByUserId);
CREATE INDEX IF NOT EXISTS IX_Tasks_Status ON Tasks(Status);
