PRAGMA foreign_keys = ON;

DELETE FROM Patients;
DELETE FROM Nurses;

INSERT INTO Nurses (Id, Name, PendingAlarms)
VALUES
    (1, 'Nurse Alice Carter', '[]'),
    (2, 'Nurse Ben Hall', '[]'),
    (3, 'Nurse Chloe Diaz', '[]'),
    (4, 'Nurse Daniel Kim', '[]');

INSERT INTO Patients (Id, Name, PrimaryNurseId, SecondaryNurseId)
VALUES
    (1, 'Patient Emma Stone', 1, 2),
    (2, 'Patient Liam Parker', 2, 3),
    (3, 'Patient Olivia Reed', 3, 4),
    (4, 'Patient Noah Brooks', 4, 1),
    (5, 'Patient Ava Morgan', 1, 3),
    (6, 'Patient Mason Price', 2, 4);