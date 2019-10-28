namespace ColdCry.AI
{
    public enum AIMovementResponse
    {
        TARGET_NULL,
        OWNER_NOT_IN_CONTACT_AREA,
        TARGET_IN_SAME_AREA,
        NO_PATH_TO_TARGET,
        PATH_FOUND,
        TARGET_MISSING_REACHABLE_COMPONENT,
        TARGET_NOT_IN_CONTACT_AREA,
        OWNER_IS_DEAD
    }
}