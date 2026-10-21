namespace TPToolkitLib.Interfaces
{
    /// <summary>
    /// Some objects depends on other objects, but they might no be loaded at this time. They can be resolved after loading everything.
    /// </summary>
    public interface IDependencyResolvable
    {
        /// <summary>
        /// Sets the references to the different objects of the class.
        /// </summary>
        public void ResolveDependency();
    }
}
