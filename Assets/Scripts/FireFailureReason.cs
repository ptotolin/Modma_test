public enum FireFailureReason
{ 
    NoReason      = 1,
    NoAmmo        = 1 << 1,
    Reloading     = 1 << 2,
    OutOfRange    = 1 << 3,
    NoTarget      = 1 << 4,
    OnCooldown    = 1 << 5,
    InvalidTarget = 1 << 6
}