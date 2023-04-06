# J4JMapLibrary: Test Routines

There are a variety of test routines defined in the `MapLibTests` project. All but one of them can be run en masse. The one which should be run standalone also requires you to run one of the other tests first.

The one which should be run standalone checks to ensure the tiles that are retrieved match some reference image files. That check is done on a byte-by-byte basis.

For the check to succeed, those reference images need to be current. They are created by the `CreateImages` test.

Here's how to run the tests:

|Test Phase|Tests to Run|
|----------|------------|
|Phase 1|CreateImages|
|Phase 2|CacheTests<br>CheckImages<br>CredentialsTest<br>ExtractTests<br>FactoryTests<br>MapTests<br>MiscTests<br>TileTests
