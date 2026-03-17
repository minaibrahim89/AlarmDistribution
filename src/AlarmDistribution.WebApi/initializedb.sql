PRAGMA foreign_keys = ON;

DELETE FROM Patients;
DELETE FROM Nurses;

INSERT INTO Nurses (Id, Name, PendingAlarms)
VALUES
    ('11111111-1111-1111-1111-111111111111', 'Nurse Alice Carter', '[]'),
    ('22222222-2222-2222-2222-222222222222', 'Nurse Ben Hall', '[]'),
    ('33333333-3333-3333-3333-333333333333', 'Nurse Chloe Diaz', '[]'),
    ('44444444-4444-4444-4444-444444444444', 'Nurse Daniel Kim', '[]');

INSERT INTO Patients (Id, Name, PrimaryNurseId, SecondaryNurseId)
VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'Patient Emma Stone', '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 'Patient Liam Parker', '22222222-2222-2222-2222-222222222222', '33333333-3333-3333-3333-333333333333'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 'Patient Olivia Reed', '33333333-3333-3333-3333-333333333333', '44444444-4444-4444-4444-444444444444'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 'Patient Noah Brooks', '44444444-4444-4444-4444-444444444444', '11111111-1111-1111-1111-111111111111'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 'Patient Ava Morgan', '11111111-1111-1111-1111-111111111111', '33333333-3333-3333-3333-333333333333'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa6', 'Patient Mason Price', '22222222-2222-2222-2222-222222222222', '44444444-4444-4444-4444-444444444444');