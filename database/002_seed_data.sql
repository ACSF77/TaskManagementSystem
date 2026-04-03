-- Task Management System - Seed Data
-- Demo users and sample tasks
-- Passwords are BCrypt hashed:
--   admin    -> Admin123!
--   johndoe  -> John123!

-- Users (BCrypt hashes generated with cost factor 11)
INSERT OR IGNORE INTO Users (Id, Username, Email, PasswordHash, CreatedAt) VALUES
('a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', 'admin', 'admin@taskboard.com', '$2a$11$PLACEHOLDER_ADMIN_HASH', '2026-01-01T00:00:00Z');

INSERT OR IGNORE INTO Users (Id, Username, Email, PasswordHash, CreatedAt) VALUES
('b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', 'johndoe', 'john@taskboard.com', '$2a$11$PLACEHOLDER_JOHN_HASH', '2026-01-15T00:00:00Z');

-- Tasks
INSERT OR IGNORE INTO Tasks (Id, Title, Description, Status, DueDate, AssignedUserId, CreatedByUserId, CreatedAt) VALUES
('c0000001-0000-4000-8000-000000000001', 'Set up project repository', 'Initialize the Git repository and configure CI/CD pipeline', 2, '2026-04-10T00:00:00Z', 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', '2026-03-01T00:00:00Z');

INSERT OR IGNORE INTO Tasks (Id, Title, Description, Status, DueDate, AssignedUserId, CreatedByUserId, CreatedAt) VALUES
('c0000001-0000-4000-8000-000000000002', 'Design database schema', 'Create the ERD and define all tables and relationships', 2, '2026-04-12T00:00:00Z', 'b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', '2026-03-05T00:00:00Z');

INSERT OR IGNORE INTO Tasks (Id, Title, Description, Status, DueDate, AssignedUserId, CreatedByUserId, CreatedAt) VALUES
('c0000001-0000-4000-8000-000000000003', 'Implement user authentication', 'Build JWT-based login and registration endpoints', 1, '2026-04-20T00:00:00Z', 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', '2026-03-10T00:00:00Z');

INSERT OR IGNORE INTO Tasks (Id, Title, Description, Status, DueDate, AssignedUserId, CreatedByUserId, CreatedAt) VALUES
('c0000001-0000-4000-8000-000000000004', 'Build task CRUD API', 'Create REST endpoints for task management with full CRUD support', 1, '2026-04-25T00:00:00Z', 'b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', '2026-03-12T00:00:00Z');

INSERT OR IGNORE INTO Tasks (Id, Title, Description, Status, DueDate, AssignedUserId, CreatedByUserId, CreatedAt) VALUES
('c0000001-0000-4000-8000-000000000005', 'Create React frontend', 'Build responsive task board UI with Kanban columns', 0, '2026-05-01T00:00:00Z', 'b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', 'b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', '2026-03-15T00:00:00Z');

INSERT OR IGNORE INTO Tasks (Id, Title, Description, Status, DueDate, AssignedUserId, CreatedByUserId, CreatedAt) VALUES
('c0000001-0000-4000-8000-000000000006', 'Write unit and integration tests', 'Achieve comprehensive test coverage across all layers', 0, '2026-05-05T00:00:00Z', NULL, 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', '2026-03-20T00:00:00Z');
